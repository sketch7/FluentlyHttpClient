using System;
using System.Data.SqlClient;
using FluentlyHttpClient.Caching;
using FluentlyHttpClient.Entity;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
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
			var conn = new SqlConnectionStringBuilder(connectionString)
			{
				ConnectRetryCount = 5,
				ConnectRetryInterval = 2,
				MaxPoolSize = 600,
				MinPoolSize = 5
			};
			//services.AddDbContext<FluentHttpClientDbContext>(options => options.UseSqlServer(conn.ToString(), builder => builder.EnableRetryOnFailure()));
			//services.AddDbContextPool<FluentHttpClientDbContext>(options => options.UseSqlServer(conn.ToString(), builder => builder.EnableRetryOnFailure()));
			//services.AddDbContextPool<FluentHttpClientDbContext>(options => options.UseSqlServer(conn.ToString(), builder => builder.EnableRetryOnFailure()));
			services.AddDbContextPool<FluentHttpClientDbContext>((sp, options) => options.UseSqlServer(conn.ToString(), builder => builder.EnableRetryOnFailure()));
			services.AddMemoryCache();

			return services;
		}
	}
}