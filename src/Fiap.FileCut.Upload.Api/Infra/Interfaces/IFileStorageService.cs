namespace Fiap.FileCut.Upload.Api.Infra.Interfaces
{
	public interface IFileService
	{
		Task<IFormFile> GetFileAsync(Guid userId, string fileName, CancellationToken cancellationToken);
		Task<IList<string>> GetFileNamesAsync(Guid userId, CancellationToken cancellationToken);
		Task<bool> SaveFileAsync(Guid userId, IFormFile file, CancellationToken cancellationToken);
		Task<bool> DeleteFileAsync(Guid userId, string fileName, CancellationToken cancellationToken);
	}
}
