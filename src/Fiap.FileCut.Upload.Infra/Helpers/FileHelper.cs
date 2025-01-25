﻿using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;

namespace Fiap.FileCut.Upload.Infra.Helpers
{
	public static class FileHelper
	{
		public static IFormFile ConvertToIFormFile(GetObjectResponse response, string fileName)
		{
			var memoryStream = new MemoryStream();
			response.ResponseStream.CopyTo(memoryStream);
			memoryStream.Position = 0;

			return new FormFile(memoryStream, 0, memoryStream.Length, fileName, fileName)
			{
				Headers = new HeaderDictionary(),
				ContentType = response.Headers.ContentType
			};
		}

		public static bool IsValidFileName(string fileName)
		{
			return !string.IsNullOrWhiteSpace(fileName) && fileName.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;
		}

		public static void ValidateFile(IFormFile file)
		{
			if (file == null || file.Length == 0)
			{
				throw new ArgumentException("File cannot be null or empty.");
			}
		}
	}
}