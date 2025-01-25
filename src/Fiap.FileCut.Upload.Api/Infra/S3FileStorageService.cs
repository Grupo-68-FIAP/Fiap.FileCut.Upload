using Fiap.FileCut.Upload.Api.Infra.Configs;
using Fiap.FileCut.Upload.Api.Infra.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace Fiap.FileCut.Upload.Api.Infra
{
	public class S3FileStorageService : IFileStorageService
	{
		private readonly S3Configs _s3Configs;
		private readonly IValidator<IFormFile> _fileValidator;

		public S3FileStorageService(IOptions<S3Configs> s3Configs, IValidator<IFormFile> fileValidator)
		{
			_s3Configs = s3Configs.Value;
			_fileValidator = fileValidator;
		}

		public async Task<string> UploadFileAsync(IFormFile file, Guid userId)
		{
			var validationResult = await _fileValidator.ValidateAsync(file);
			if (!validationResult.IsValid)
			{
				throw new InvalidOperationException(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
			}

			var safeFileName = GenerateSafeFileName(file, userId);
			var filePath = await SaveFileAsync(file, safeFileName);

			return $"{_s3Configs.S3Url}/{_s3Configs.BucketName}/{safeFileName}";
		}

		private string GenerateSafeFileName(IFormFile file, Guid userId)
		{
			var fileExtension = Path.GetExtension(file.FileName);
			var safeFileName = $"{userId}_{Guid.NewGuid()}{fileExtension}";
			return safeFileName;
		}

		private async Task<string> SaveFileAsync(IFormFile file, string fileName)
		{
			var filePath = Path.Combine(Directory.GetCurrentDirectory(), "UploadedVideos", fileName);
			filePath = Path.GetFullPath(filePath);

			if (!filePath.StartsWith(Path.GetFullPath(Directory.GetCurrentDirectory())))
				throw new InvalidOperationException("Caminho de arquivo inválido.");

			Directory.CreateDirectory(Path.GetDirectoryName(filePath));

			using (var stream = new FileStream(filePath, FileMode.Create))
			{
				await file.CopyToAsync(stream);
			}

			return filePath;
		}
	}
}