using FluentlyHttpClient.Caching;
using FluentlyHttpClient.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Data.SqlClient;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>DI registration extensions for <c>FluentlyHttpClient.Entity</c>.</summary>
public static class ServiceCollectionExtensions
{
	/// <summary>
	/// Register FluentlyHttpClient Entity services (SQL Server-backed response cache) using the given connection string.
	/// </summary>
	/// <param name="services">The service collection.</param>
	/// <param name="connectionString">SQL Server connection string.</param>
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

	/// <summary>
	/// Register FluentlyHttpClient Entity services using a pre-built <see cref="SqlConnectionStringBuilder"/>.
	/// </summary>
	/// <param name="services">The service collection.</param>
	/// <param name="connectionStringBuilder">Pre-configured connection string builder.</param>
	/// <param name="builder">Optional SQL Server options builder (e.g. retry policy).</param>
	public static IServiceCollection AddFluentlyHttpClientEntity(
		this IServiceCollection services,
		SqlConnectionStringBuilder connectionStringBuilder,
		Action<SqlServerDbContextOptionsBuilder>? builder = null
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