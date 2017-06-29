using System;
using FluentlyHttpClient;
using System.Net.Http;
using FluentlyHttpClient.Test;
using Xunit;
using static FluentlyHttpClient.Test.ServiceTestUtil;

namespace Test
{
	public class ClientFactory_WithRequestBuilderDefaults
	{
		[Fact]
		public void WithCustomRequestDefaults()
		{
			var fluentHttpClientFactory = GetNewClientFactory();
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
			var fluentHttpClientFactory = GetNewClientFactory();
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
			var fluentHttpClientFactory = GetNewClientFactory();
			var clientBuilder = fluentHttpClientFactory.CreateBuilder(null);
			Assert.Throws<ClientBuilderValidationException>(() => clientBuilder.Register());
		}

		[Fact]
		public void ThrowsErrorWhenUriNotSpecified()
		{
			var fluentHttpClientFactory = GetNewClientFactory();
			var clientBuilder = fluentHttpClientFactory.CreateBuilder("abc");

			Assert.Throws<ClientBuilderValidationException>(() => clientBuilder.Register());
		}

		[Fact]
		public void ThrowsErrorWhenAlreadyRegistered()
		{
			var fluentHttpClientFactory = GetNewClientFactory();
			var clientBuilder = fluentHttpClientFactory.CreateBuilder("abc")
					.WithBaseUrl("http://abc.com")
					.Register();

			Assert.Throws<ClientBuilderValidationException>(() => clientBuilder.Register());
		}
	}

	public class ClientFactory_Remove
	{
		[Fact]
		public async void ShouldDisposeClient()
		{
			var fluentHttpClientFactory = GetNewClientFactory();
			fluentHttpClientFactory.CreateBuilder("abc")
				.WithBaseUrl("http://abc.com")
				.Register();

			var httpClient = fluentHttpClientFactory.Get("abc");
			var isRegistered = fluentHttpClientFactory.Remove("abc")
									.Has("abc");

			await Assert.ThrowsAsync<ObjectDisposedException>(() => httpClient.Get<Hero>("/api/heroes/azmodan"));
			Assert.False(isRegistered);
		}
	}

}