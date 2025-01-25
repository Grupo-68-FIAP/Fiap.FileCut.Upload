namespace Fiap.FileCut.Upload.Api.Infra.Interfaces
{
	public interface IFileStorageService
	{
		Task<string> UploadFileAsync(IFormFile file, Guid userId);
	}
}
