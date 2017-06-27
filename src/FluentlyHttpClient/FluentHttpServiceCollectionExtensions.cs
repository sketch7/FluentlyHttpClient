using FluentlyHttpClient;
using System;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
	/// <summary>
	/// Extension methods for setting up fluent http services in an <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
	/// </summary>
	public static class FluentHttpServiceCollectionExtensions
	{
		/// <summary>
		/// Adds fluently http client services to the specified <see cref="IServiceCollection"/>.
		/// </summary>
		/// <param name="services"></param>
		/// <returns>Returns service collection for chaining.</returns>
		public static IServiceCollection AddFluentlyHttpClient(this IServiceCollection services)
		{
			if (services == null) throw new ArgumentNullException(nameof(services));

			services.AddSingleton<FluentHttpClientFactory>();
			services.AddSingleton<IFluentHttpMiddlewareRunner, FluentHttpMiddlewareRunner>();

			return services;
		}
	}
}