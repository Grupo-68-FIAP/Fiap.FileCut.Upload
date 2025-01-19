
using Fiap.FileCut.Infra.Api.Configurations;

namespace Fiap.FileCut.Upload.Api
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddJwtBearerAuthentication();
            builder.Services.AddSwaggerGen();
            builder.Services.AddEnvCors();

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
