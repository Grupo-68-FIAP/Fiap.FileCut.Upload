﻿using Amazon.S3;
using Amazon.S3.Model;

namespace Fiap.FileCut.Upload.Infra.Helpers
{
	public class S3Helper
	{
		private readonly IAmazonS3 _s3Client;
		private readonly string _bucketName;

		public S3Helper(IAmazonS3 s3Client, string bucketName)
		{
			_s3Client = s3Client;
			_bucketName = bucketName;
		}

		public async Task<GetObjectResponse> DownloadS3ObjectAsync(Guid userId, string fileName, CancellationToken cancellationToken)
		{
			var key = GetS3Key(userId, fileName);
			var request = new GetObjectRequest
			{
				BucketName = _bucketName,
				Key = key
			};

			return await _s3Client.GetObjectAsync(request, cancellationToken);
		}

		public string GetS3Key(Guid userId, string fileName)
		{
			return Path.Combine(userId.ToString(), fileName).Replace("\\", "/");
		}
	}
}