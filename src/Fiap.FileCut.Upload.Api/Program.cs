using Fiap.FileCut.Infra.Api;

namespace Fiap.FileCut.Upload.Api
{
	public static class Program
    {
        public static async Task Main(string[] args)
        {
			var builder = WebApplication.CreateBuilder(args);

			builder.Services.AddControllers();
			builder.Services.AddEndpointsApiExplorer();

			await builder.Services.ConfigureFileCutUploadApi();

			var app = builder.Build();

			await app.InitializeFileCutUploadApi();
			app.MapControllers();

			var scope = app.Services.CreateScope();
			await scope.ScopedFileCutUploadApi();

			await app.RunAsync();
		}
    }
}