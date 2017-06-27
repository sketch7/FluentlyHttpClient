using FluentlyHttpClient;
using System;
using FluentlyHttpClient.Middleware;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
	/// <summary>
	/// Extension methods for setting up fluent http services in an <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
	/// </summary>
	public static class FluentlyHttpClientServiceCollectionExtensions
	{
		/// <summary>
		/// Adds fluently http client services to the specified <see cref="IServiceCollection"/>.
		/// </summary>
		/// <param name="services"></param>
		/// <returns>Returns service collection for chaining.</returns>
		public static IServiceCollection AddFluentlyHttpClient(this IServiceCollection services)
		{
			if (services == null) throw new ArgumentNullException(nameof(services));

			services.AddSingleton<IFluentHttpClientFactory, FluentHttpClientFactory>();
			services.AddSingleton<IFluentHttpMiddlewareRunner, FluentHttpMiddlewareRunner>();

			return services;
		}
	}
}