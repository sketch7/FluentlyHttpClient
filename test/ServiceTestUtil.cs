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
	}
}