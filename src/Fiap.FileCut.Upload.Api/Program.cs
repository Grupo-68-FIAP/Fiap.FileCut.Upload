using Fiap.FileCut.Upload.Api.Extensions;
using Fiap.FileCut.Upload.Infra.Extensions;

namespace Fiap.FileCut.Upload.Api
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers(); 
            builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddJwtBearerAuthentication();
			builder.Services.AddSwaggerGen();
            builder.Services.AddEnvCors();
            builder.Services.AddS3Services(builder.Configuration);

			var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseEnvCors();

            app.UseHttpsRedirection();

            app.UseAuth();

            app.MapControllers();

            app.Run();
        }
    }
}
