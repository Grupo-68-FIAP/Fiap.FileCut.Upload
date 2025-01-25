namespace Fiap.FileCut.Upload.Domain.Exceptions
{
	public class FileRepositoryException : Exception
	{
		public FileRepositoryException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}