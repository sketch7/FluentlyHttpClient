using System;
using FluentHttpClient.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddFluentlyHttpClientEntity(this IServiceCollection services, string connectionString)
		{
			if (string.IsNullOrWhiteSpace(connectionString))
				throw new ArgumentNullException(nameof(connectionString));

			//connectionString = @"Data Source=.\SQLEXPRESS;Database=FluentHttpClient;Integrated Security=True";

			services.TryAddTransient<IRemoteResponseCacheService, RemoteResponseCacheService>();
			services.AddDbContext<FluentHttpClientContext>(options => options.UseSqlServer(connectionString));

			return services;
		}
	}
}