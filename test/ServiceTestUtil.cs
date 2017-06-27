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
		/// Create a new container and return FluentHttpClientFactory.
		/// </summary>
		/// <returns></returns>
		public static FluentHttpClientFactory GetNewClientFactory()
		{
			var serviceProvider = CreateContainer().BuildServiceProvider();
			var fluentHttpClientFactory = serviceProvider.GetService<FluentHttpClientFactory>();
			return fluentHttpClientFactory;
		}
	}
}