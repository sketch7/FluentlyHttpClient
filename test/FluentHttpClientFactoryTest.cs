using System.Net.Http;
using FluentlyHttpClient;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using static Test.ClientFactoryTestUtil;

namespace Test
{
	public static class ClientFactoryTestUtil
	{
		public static IServiceCollection CreateContainer()
			=> new ServiceCollection()
			.AddFluentlyHttpClient();

		/// <summary>
		/// Create a new container and return FluentHttpClientFactory.
		/// </summary>
		/// <returns></returns>
		public static FluentHttpClientFactory GetClientFactory()
		{
			var serviceProvider = CreateContainer().BuildServiceProvider();
			var fluentHttpClientFactory = serviceProvider.GetService<FluentHttpClientFactory>();
			return fluentHttpClientFactory;
		}
	}

	public class ClientFactory_WithRequestBuilderDefaults
	{
		[Fact]
		public void WithCustomRequestDefaults()
		{
			var fluentHttpClientFactory = GetClientFactory();
			fluentHttpClientFactory.CreateBuilder("abc")
				.WithBaseUrl("http://abc.com")
				.WithRequestBuilderDefaults(builder => builder.AsPut())
				.Register();

			var httpClient = fluentHttpClientFactory.Get("abc");
			var request = httpClient.CreateRequest("/api")
				.Build();

			Assert.NotNull(request);
			Assert.Equal(HttpMethod.Put, request.Method);
		}
	}

	public class ClientFactory_Register
	{
		[Fact]
		public void ShouldRegisterSuccessfully()
		{
			var fluentHttpClientFactory = GetClientFactory();
			fluentHttpClientFactory.CreateBuilder("abc")
				.WithBaseUrl("http://abc.com")
				.Register();

			var httpClient = fluentHttpClientFactory.Get("abc");
			Assert.NotNull(httpClient);
			Assert.Equal("abc", httpClient.Identifier);
		}

		[Fact]
		public void ThrowsErrorWhenIdentifierNotSpecified()
		{
			var fluentHttpClientFactory = GetClientFactory();
			var clientBuilder = fluentHttpClientFactory.CreateBuilder(null);
			Assert.Throws<ClientBuilderValidationException>(() => clientBuilder.Register());
		}

		[Fact]
		public void ThrowsErrorWhenUriNotSpecified()
		{
			var fluentHttpClientFactory = GetClientFactory();
			var clientBuilder = fluentHttpClientFactory.CreateBuilder("abc");

			Assert.Throws<ClientBuilderValidationException>(() => clientBuilder.Register());
		}

		[Fact]
		public void ThrowsErrorWhenAlreadyRegistered()
		{
			var fluentHttpClientFactory = GetClientFactory();
			var clientBuilder = fluentHttpClientFactory.CreateBuilder("abc")
					.WithBaseUrl("http://abc.com")
					.Register();

			Assert.Throws<ClientBuilderValidationException>(() => clientBuilder.Register());
		}
	}

}