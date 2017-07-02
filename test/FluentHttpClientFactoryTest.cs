﻿using FluentlyHttpClient;
using FluentlyHttpClient.Test;
using System;
using System.Linq;
using System.Net.Http;
using Xunit;
using static FluentlyHttpClient.Test.ServiceTestUtil;

namespace Test
{
	public class ClientFactory_WithRequestBuilderDefaults
	{
		[Fact]
		public void ShouldHaveWithCustomDefaultsSet()
		{
			var fluentHttpClientFactory = GetNewClientFactory();
			var clientBuilder = fluentHttpClientFactory.CreateBuilder("abc")
				.WithBaseUrl("http://abc.com")
				.WithRequestBuilderDefaults(builder => builder.AsPut());

			var httpClient = fluentHttpClientFactory.Add(clientBuilder);
			var request = httpClient.CreateRequest("/api")
				.Build();

			Assert.NotNull(request);
			Assert.Equal(HttpMethod.Put, request.Method);
		}
	}

	public class ClientFactory_WithFormatters
	{
		[Fact]
		public void ShouldSetClientFormatters()
		{
			var fluentHttpClientFactory = GetNewClientFactory();
			var clientBuilder = fluentHttpClientFactory.CreateBuilder("abc")
				.WithBaseUrl("http://abc.com")
				.WithFormatters(formatter => formatter.Clear());

			var httpClient = fluentHttpClientFactory.Add(clientBuilder);

			Assert.Equal(0, httpClient.Formatters.Count);
		}
	}
	public class ClientFactory_WithConfigureDefaults
	{
		[Fact]
		public void ShouldSetClientFormatters()
		{
			var fluentHttpClientFactory = GetNewClientFactory();
			var clientBuilder = fluentHttpClientFactory.ConfigureDefaults(builder => builder.WithUserAgent("hots"))
				.CreateBuilder("abc")
				.WithBaseUrl("http://abc.com");

			var httpClient = fluentHttpClientFactory.Add(clientBuilder);

			var userAgentHeader = httpClient.Headers.GetValues("User-Agent").FirstOrDefault();
			Assert.Equal("hots", userAgentHeader);
		}
	}

	public class ClientFactory_Register
	{
		[Fact]
		public void ShouldRegisterSuccessfully()
		{
			var fluentHttpClientFactory = GetNewClientFactory();
			var clientBuilder = fluentHttpClientFactory.CreateBuilder("abc")
				.WithBaseUrl("http://abc.com");

			var httpClient = fluentHttpClientFactory.Add(clientBuilder);
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
			var clientBuilder = fluentHttpClientFactory.CreateBuilder("abc")
				.WithBaseUrl("http://abc.com");

			var httpClient = fluentHttpClientFactory.Add(clientBuilder);
			var isRegistered = fluentHttpClientFactory.Remove("abc")
									.Has("abc");

			await Assert.ThrowsAsync<ObjectDisposedException>(() => httpClient.Get<Hero>("/api/heroes/azmodan"));
			Assert.False(isRegistered);
		}
	}

}