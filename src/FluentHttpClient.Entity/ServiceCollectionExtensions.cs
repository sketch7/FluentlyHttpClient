using System;
using FluentHttpClient.Entity;
using FluentlyHttpClient.Caching;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.Extensions.DependencyInjection
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddFluentlyHttpClientEntity(this IServiceCollection services, string connectionString)
		{
			if (string.IsNullOrWhiteSpace(connectionString))
				throw new ArgumentNullException(nameof(connectionString));

			//connectionString = @"Data Source=.\SQLEXPRESS;Database=FluentHttpClient;Integrated Security=True";

			services.AddSingleton<IResponseCacheService, RemoteResponseCacheService>();
			services.AddDbContext<FluentHttpClientDbContext>(options => options.UseSqlServer(connectionString));
			services.AddMemoryCache();

			return services;
		}
	}
}