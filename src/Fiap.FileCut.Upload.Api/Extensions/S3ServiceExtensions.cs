using Amazon.Runtime;
using Amazon.S3;
using Fiap.FileCut.Upload.Domain.Interfaces;
using Fiap.FileCut.Upload.Infra.Configs;
using Fiap.FileCut.Upload.Infra.Repositories.S3;
using Microsoft.Extensions.Options;

namespace Fiap.FileCut.Upload.Infra.Extensions
{
	public static class S3ServiceExtensions
	{
		public static IServiceCollection AddS3Services(this IServiceCollection services, IConfiguration configuration)
		{
			services.Configure<S3Settings>(configuration.GetSection("AWS"));

			services.AddSingleton<IAmazonS3>(sp =>
			{
				var s3Settings = sp.GetRequiredService<IOptions<S3Settings>>().Value;

				return new AmazonS3Client(
					new BasicAWSCredentials(s3Settings.AccessKey, s3Settings.SecretKey),
					Amazon.RegionEndpoint.GetBySystemName(s3Settings.Region)
				);
			});

			services.AddScoped<IFileRepository, S3FileRepository>();

			return services;
		}
	}
}
