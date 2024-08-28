using FluentlyHttpClient;
using FluentlyHttpClient.Caching;
using FluentlyHttpClient.Middleware;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for setting up fluent HTTP services in an <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
/// </summary>
public static class FluentlyHttpClientServiceCollectionExtensions
{
	/// <summary>
	/// Adds fluently HTTP client services to the specified <see cref="IServiceCollection"/>.
	/// </summary>
	/// <param name="services"></param>
	/// <param name="configureDefaults">Configure defaults.</param>
	/// <returns>Returns service collection for chaining.</returns>
	public static IServiceCollection AddFluentlyHttpClient(
		this IServiceCollection services,
		Action<FluentHttpClientBuilder>? configureDefaults = null
	)
	{
		ArgumentNullException.ThrowIfNull(services);

		services.TryAddSingleton<IHttpResponseSerializer, HttpResponseSerializer>();
		services.TryAddSingleton<IFluentHttpClientFactory, FluentHttpClientFactory>();
		services.TryAddTransient<FluentHttpMiddlewareBuilder>();
		services.AddMemoryCache();
		services.TryAddSingleton<IResponseCacheService, MemoryResponseCacheService>();
		services.TryAddSingleton(new FluentHttpClientFactoryOptions(configureDefaults));

		services.AddHttpClient();

		return services;
	}
}