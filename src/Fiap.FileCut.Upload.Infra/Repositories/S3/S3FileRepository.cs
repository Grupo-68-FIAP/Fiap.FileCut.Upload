﻿using Amazon.S3;
using Amazon.S3.Model;
using Fiap.FileCut.Upload.Domain.Exceptions;
using Fiap.FileCut.Upload.Domain.Interfaces;
using Fiap.FileCut.Upload.Infra.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Fiap.FileCut.Upload.Infra.Repositories.S3
{
	public class S3FileRepository : IFileRepository
	{
		private readonly IAmazonS3 _s3Client;
		private readonly string _bucketName;
		private readonly ILogger<S3FileRepository> _logger;
		private readonly S3Helper _s3Helper;

		public S3FileRepository(IAmazonS3 s3Client, string bucketName, ILogger<S3FileRepository> logger, S3Helper s3Helper)
		{
			_s3Client = s3Client;
			_bucketName = bucketName;
			_logger = logger;
			_s3Helper = s3Helper;
		}

		public async Task<IFormFile> GetAsync(Guid userId, string fileName, CancellationToken cancellationToken)
		{
			try
			{
				if (!FileHelper.IsValidFileName(fileName))
				{
					_logger.LogWarning("[{source}] - Invalid file name: {FileName}", nameof(S3FileRepository), fileName);
					throw new ArgumentException("Invalid file name.", nameof(fileName));
				}

				_logger.LogInformation("[{source}] - Starting file download. User: {UserId}, File: {FileName}", nameof(S3FileRepository), userId, fileName);

				var s3Object = await _s3Helper.DownloadS3ObjectAsync(userId, fileName, cancellationToken);
				if (s3Object == null || s3Object.ResponseStream == null)
				{
					_logger.LogWarning("[{source}] - S3 object is null or response stream is missing. User: {UserId}, File: {FileName}", nameof(S3FileRepository), userId, fileName);
					throw new FileNotFoundException($"The file '{fileName}' was not found in S3 for user '{userId}'.");
				}

				_logger.LogInformation("[{source}] - File downloaded successfully. User: {UserId}, File: {FileName}", nameof(S3FileRepository), userId, fileName);

				return FileHelper.ConvertToIFormFile(s3Object, fileName);
			}
			catch (AmazonS3Exception s3Ex)
			{
				_logger.LogError(s3Ex, "[{source}] - AWS S3 error while downloading file. User: {UserId}, File: {FileName}", nameof(S3FileRepository), userId, fileName);
				throw new FileRepositoryException("Error downloading file from S3.", s3Ex);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[{source}] - Unexpected error while downloading file. User: {UserId}, File: {FileName}", nameof(S3FileRepository), userId, fileName);
				throw;
			}
		}

		public async Task<IList<IFormFile>> GetAllAsync(Guid userId, CancellationToken cancellationToken = default)
		{
			try
			{
				_logger.LogInformation("[{source}] - Starting file listing. User: {UserId}", nameof(S3FileRepository), userId);

				var request = new ListObjectsV2Request
				{
					BucketName = _bucketName,
					Prefix = $"{userId}/"
				};

				var response = await _s3Client.ListObjectsV2Async(request, cancellationToken);
				if (response.S3Objects == null || !response.S3Objects.Any())
				{
					_logger.LogWarning("[{source}] - No files found for user {UserId}", nameof(S3FileRepository), userId);
					return new List<IFormFile>();
				}

				var files = new List<IFormFile>();
				foreach (var obj in response.S3Objects)
				{
					var fileName = obj.Key.Replace($"{userId}/", "");
					files.Add(await GetAsync(userId, fileName, cancellationToken));
				}

				_logger.LogInformation("[{source}] - Successfully listed {FileCount} files for user {UserId}", nameof(S3FileRepository), files.Count, userId);
				return files;
			}
			catch (AmazonS3Exception s3Ex)
			{
				_logger.LogError(s3Ex, "[{source}] - AWS S3 error while listing files. User: {UserId}", nameof(S3FileRepository), userId);
				throw new FileRepositoryException("Error listing files from S3.", s3Ex);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[{source}] - Unexpected error while listing files. User: {UserId}", nameof(S3FileRepository), userId);
				throw;
			}
		}

		public async Task<bool> UpdateAsync(Guid userId, IFormFile file, CancellationToken cancellationToken = default)
		{
			try
			{
				if (!FileHelper.IsValidFileName(file.FileName))
				{
					_logger.LogWarning("[{source}] - Invalid file name: {FileName}", nameof(S3FileRepository), file.FileName);
					throw new ArgumentException("Invalid file name.", nameof(file.FileName));
				}

				_logger.LogInformation("[{source}] - Starting file upload/update. User: {UserId}, File: {FileName}", nameof(S3FileRepository), userId, file.FileName);

				var key = _s3Helper.GetS3Key(userId, file.FileName);
				using var stream = file.OpenReadStream();

				var request = new PutObjectRequest
				{
					BucketName = _bucketName,
					Key = key,
					InputStream = stream,
					ContentType = file.ContentType
				};

				var response = await _s3Client.PutObjectAsync(request, cancellationToken);
				if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
				{
					_logger.LogInformation("[{source}] - File uploaded/updated successfully. User: {UserId}, File: {FileName}", nameof(S3FileRepository), userId, file.FileName);
					return true;
				}

				_logger.LogWarning("[{source}] - File upload/update failed. User: {UserId}, File: {FileName}, Status: {HttpStatus}", nameof(S3FileRepository), userId, file.FileName, response.HttpStatusCode);
				return false;
			}
			catch (AmazonS3Exception s3Ex)
			{
				_logger.LogError(s3Ex, "[{source}] - AWS S3 error while uploading/updating file. User: {UserId}, File: {FileName}", nameof(S3FileRepository), userId, file.FileName);
				throw new FileRepositoryException("Error uploading file to S3.", s3Ex);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[{source}] - Unexpected error while uploading/updating file. User: {UserId}, File: {FileName}", nameof(S3FileRepository), userId, file.FileName);
				throw;
			}
		}

		public async Task<bool> DeleteAsync(Guid userId, string fileName, CancellationToken cancellationToken = default)
		{
			try
			{
				if (!FileHelper.IsValidFileName(fileName))
				{
					_logger.LogWarning("[{source}] - Invalid file name: {FileName}", nameof(S3FileRepository), fileName);
					throw new ArgumentException("Invalid file name.", nameof(fileName));
				}

				_logger.LogInformation("[{source}] - Starting file deletion. User: {UserId}, File: {FileName}", nameof(S3FileRepository), userId, fileName);

				var key = _s3Helper.GetS3Key(userId, fileName);
				var request = new DeleteObjectRequest
				{
					BucketName = _bucketName,
					Key = key
				};

				var response = await _s3Client.DeleteObjectAsync(request, cancellationToken);
				if (response.HttpStatusCode == System.Net.HttpStatusCode.NoContent)
				{
					_logger.LogInformation("[{source}] - File deleted successfully. User: {UserId}, File: {FileName}", nameof(S3FileRepository), userId, fileName);
					return true;
				}

				_logger.LogWarning("[{source}] - File deletion failed. User: {UserId}, File: {FileName}, Status: {HttpStatus}", nameof(S3FileRepository), userId, fileName, response.HttpStatusCode);
				return false;
			}
			catch (AmazonS3Exception s3Ex)
			{
				_logger.LogError(s3Ex, "[{source}] - AWS S3 error while deleting file. User: {UserId}, File: {FileName}", nameof(S3FileRepository), userId, fileName);
				throw new FileRepositoryException("Error deleting file from S3.", s3Ex);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[{source}] - Unexpected error while deleting file. User: {UserId}, File: {FileName}", nameof(S3FileRepository), userId, fileName);
				throw;
			}
		}
	}
}