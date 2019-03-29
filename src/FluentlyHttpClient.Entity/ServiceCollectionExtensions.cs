using System;
using System.Data.SqlClient;
using FluentlyHttpClient.Caching;
using FluentlyHttpClient.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddFluentlyHttpClientEntity(this IServiceCollection services, string connectionString)
		{
			if (string.IsNullOrWhiteSpace(connectionString))
				throw new ArgumentNullException(nameof(connectionString));

			var conn = new SqlConnectionStringBuilder(connectionString)
			{
				ConnectRetryCount = 5,
				ConnectRetryInterval = 2,
				MaxPoolSize = 600,
				MinPoolSize = 5
			};

			return services.AddFluentlyHttpClientEntity(conn, builder => builder.EnableRetryOnFailure());
		}

		public static IServiceCollection AddFluentlyHttpClientEntity(
			this IServiceCollection services,
			SqlConnectionStringBuilder connectionStringBuilder,
			Action<SqlServerDbContextOptionsBuilder> builder = null
		)
		{
			if (connectionStringBuilder == null)
				throw new ArgumentNullException(nameof(SqlConnectionStringBuilder));

			services.AddSingleton<IResponseCacheService, RemoteResponseCacheService>();

			services.AddDbContextPool<FluentHttpClientDbContext>(options
				=> options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
					.UseSqlServer(connectionStringBuilder.ToString(), builder)
			);
			services.AddMemoryCache();

			return services;
		}
	}
}