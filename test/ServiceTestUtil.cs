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
		/// <returns></returns>
		public static IFluentHttpClientFactory GetNewClientFactory()
		{
			var serviceProvider = CreateContainer().BuildServiceProvider();
			var fluentHttpClientFactory = serviceProvider.GetService<IFluentHttpClientFactory>();
			return fluentHttpClientFactory;
		}

		/// <summary>
		/// Create a new container, configure http client and create new request.
		/// </summary>
		/// <returns></returns>
		public static FluentHttpRequestBuilder GetNewRequestBuilder(string uri = "/api")
		{
			var serviceProvider = CreateContainer().BuildServiceProvider();
			var fluentHttpClientFactory = serviceProvider.GetService<IFluentHttpClientFactory>();
			var clientBuilder = fluentHttpClientFactory.CreateBuilder("sketch7")
				.WithBaseUrl("https://sketch7.com");

			var httpClient = fluentHttpClientFactory.Add(clientBuilder);
			return httpClient.CreateRequest(uri);
		}
	}
}