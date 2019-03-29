using System;
using FluentlyHttpClient;
using FluentlyHttpClient.Caching;
using FluentlyHttpClient.Middleware;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
	/// <summary>
	/// Extension methods for setting up fluent HTTP services in an <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
	/// </summary>
	public static class FluentlyHttpClientServiceCollectionExtensions
	{
		/// <summary>
		/// Adds fluently HTTP client services to the specified <see cref="IServiceCollection"/>.
		/// </summary>
		/// <param name="services"></param>
		/// <returns>Returns service collection for chaining.</returns>
		public static IServiceCollection AddFluentlyHttpClient(this IServiceCollection services)
		{
			if (services == null) throw new ArgumentNullException(nameof(services));

			services.TryAddSingleton<IHttpResponseSerializer, HttpResponseSerializer>();
			services.TryAddSingleton<IFluentHttpClientFactory, FluentHttpClientFactory>();
			services.TryAddTransient<FluentHttpMiddlewareBuilder>();
			services.AddMemoryCache();
			services.TryAddSingleton<IResponseCacheService, MemoryResponseCacheService>();

			services.AddHttpClient();

			return services;
		}
	}
}