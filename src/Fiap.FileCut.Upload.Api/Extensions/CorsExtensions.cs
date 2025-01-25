﻿namespace Fiap.FileCut.Upload.Api.Extensions
{
	/// <summary>
	/// Cors configuration class.
	/// </summary>
	public static class CorsExtensions
	{
		/// <summary>
		/// Allow origins. Default is "*".
		/// Allow origins separated by comma.
		/// Example: "http://localhost:3000,http://localhost:3001"
		/// </summary>
		private static string allowOrigins = "*";
		/// <summary>
		/// Allow methods. Default is "*".
		/// Allow methods separated by comma.
		/// Example: "GET,POST,PUT,DELETE"
		/// </summary>
		private static string allowMethod = "*";
		/// <summary>
		/// Allow headers. Default is "*".
		/// Allow headers separated by comma.
		/// Example: "Content-Type,Authorization"
		/// </summary>
		private static string allowHeaders = "*";

		/// <summary>
		/// Add Cors configuration to the application.
		/// </summary>
		/// <param name="services">ServiceCollection instance.</param>
		public static void AddEnvCors(this IServiceCollection services)
		{
			services.AddCors(options =>
			{
				allowOrigins = Environment.GetEnvironmentVariable("ALLOW_ORIGINS") ?? allowOrigins;
				allowMethod = Environment.GetEnvironmentVariable("ALLOW_METHOD") ?? allowMethod;
				allowHeaders = Environment.GetEnvironmentVariable("ALLOW_HEADERS") ?? allowHeaders;

				options.AddPolicy("CorsConfig", builder =>
				{
					builder.WithOrigins(allowOrigins.Split(','));
					builder.WithMethods(allowMethod.Split(","));
					builder.WithHeaders(allowHeaders.Split(","));
				});
			});
		}

		/// <summary>
		/// Use Cors configuration to the application.
		/// </summary>
		/// <param name="app">WebApplication instance.</param>
		public static void UseEnvCors(this IApplicationBuilder app)
		{
			app.UseCors("CorsConfig");
		}
	}
}
