using System;
using Microsoft.Extensions.DependencyInjection;

namespace FluentlyHttpClient.Test
{
	public static class ServiceTestUtil
	{
		public static IServiceCollection CreateContainer()
			=> new ServiceCollection()
				.AddFluentlyHttpClient()
				.AddLogging();

		/// <summary>
		/// Create a new container and return IFluentHttpClientFactory.
		/// </summary>
		public static IFluentHttpClientFactory GetNewClientFactory(Action<IServiceCollection> configureContainer = null)
		{
			var container = CreateContainer();
			configureContainer?.Invoke(container);
			var serviceProvider = container.BuildServiceProvider();
			return serviceProvider.GetService<IFluentHttpClientFactory>();
		}

		/// <summary>
		/// Create a new container, configure http client and create new request.
		/// </summary>
		public static FluentHttpRequestBuilder GetNewRequestBuilder(string uri = "/api", Action<FluentHttpClientBuilder> configureClient = null)
		{
			var fluentHttpClientFactory = GetNewClientFactory();
			var clientBuilder = fluentHttpClientFactory.CreateBuilder("sketch7")
				.WithBaseUrl("https://sketch7.com")
				;

			configureClient?.Invoke(clientBuilder);

			var httpClient = fluentHttpClientFactory.Add(clientBuilder);
			return httpClient.CreateRequest(uri);
		}
	}
}