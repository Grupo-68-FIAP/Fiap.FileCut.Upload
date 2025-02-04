﻿using Fiap.FileCut.Core.Interfaces.Repository;
using Fiap.FileCut.Upload.Api.Infra.Interfaces;

namespace Fiap.FileCut.Upload.Api.Infra
{
	public class FileService : IFileService
	{
		private readonly IFileRepository _fileRepository;
		private readonly ILogger<FileService> _logger;

		public FileService(
			IFileRepository fileRepository,
			ILogger<FileService> logger)
		{
			_fileRepository = fileRepository;
			_logger = logger;
		}

		public async Task<bool> DeleteFileAsync(Guid userId, string fileName, CancellationToken cancellationToken)
		{
			try
			{
				_logger.LogInformation("[{source}] - Starting file deletion. User: {UserId}, File: {FileName}", nameof(FileService), userId, fileName);

				var result = await _fileRepository.DeleteAsync(userId, fileName);

				if (result)
					_logger.LogInformation("[{source}] - File deleted successfully. User: {UserId}, File: {FileName}", nameof(FileService), userId, fileName);
				else
					_logger.LogWarning("[{source}] - Failed to delete file. User: {UserId}, File: {FileName}", nameof(FileService), userId, fileName);

				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[{source}] - Unexpected error while deleting file. User: {UserId}, File: {FileName}", nameof(FileService), userId, fileName);
				throw;
			}
		}

		public async Task<IFormFile> GetFileAsync(Guid userId, string fileName, CancellationToken cancellationToken)
		{
			try
			{
				_logger.LogInformation("[{source}] - Starting file download. User: {UserId}, File: {FileName}", nameof(FileService), userId, fileName);

				return await _fileRepository.GetAsync(userId, fileName);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[{source}] - Error while getting the file. User: {UserId}, File: {FileName}", nameof(FileService), userId, fileName);
				throw;
			}
		}

		public async Task<IList<string>> GetFileNamesAsync(Guid userId, CancellationToken cancellationToken)
		{
			try
			{
				_logger.LogInformation("[{source}] - Starting file name listing. User: {UserId}", nameof(FileService), userId);

				var files = await _fileRepository.GetAllAsync(userId);
				var fileNames = files.Select(file => file.FileName).ToList();

				_logger.LogInformation("[{source}] - Successfully listed {FileCount} file names for user {UserId}", nameof(FileService), fileNames.Count(), userId);

				return fileNames;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[{source}] - Error while listing file names. User: {UserId}", nameof(FileService), userId);
				throw;
			}
		}

		public async Task<bool> SaveFileAsync(Guid userId, IFormFile file, CancellationToken cancellationToken)
		{
			try
			{
				if (file.Length <= 0)
					throw new ArgumentException("Invalid file size");

				_logger.LogInformation("[{source}] - Starting file upload. User: {UserId}, File: {FileName}", nameof(FileService), userId, file.FileName);

				var result = await _fileRepository.UpdateAsync(userId, file);
				if (result)
					_logger.LogInformation("[{source}] - File saved successfully. User: {UserId}, File: {FileName}", nameof(FileService), userId, file.FileName);
				else
					_logger.LogWarning("[{source}] - Failed to save file. User: {UserId}, File: {FileName}", nameof(FileService), userId, file.FileName);

				return result;
			}
			catch (ArgumentException ex)
			{
				_logger.LogError(ex, "[{source}] - Validation error while saving file. User: {UserId}, File: {FileName}", nameof(FileService), userId, file.FileName);
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[{source}] - Unexpected error while saving file. User: {UserId}, File: {FileName}", nameof(FileService), userId, file.FileName);
				throw;
			}
		}
	}
}