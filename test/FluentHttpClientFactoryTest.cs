using FluentlyHttpClient;
using FluentlyHttpClient.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
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

		[Fact]
		public void ShouldHaveQueryParamsDefaultsSet()
		{
			var fluentHttpClientFactory = GetNewClientFactory();
			var clientBuilder = fluentHttpClientFactory.CreateBuilder("abc")
				.WithBaseUrl("http://abc.com")
				.WithRequestBuilderDefaults(builder =>
					builder.WithQueryParamsOptions(opts =>
					{
						opts.CollectionMode = QueryStringCollectionMode.CommaSeparated;
						opts.KeyFormatter = key => key.ToUpper();
					})
				);

			var httpClient = fluentHttpClientFactory.Add(clientBuilder);
			var request = httpClient.CreateRequest("/api/heroes")
				.WithQueryParams(new
				{
					Roles = new List<string> { "warrior", "assassin" },
				})
				.Build();

			Assert.Equal("/api/heroes?ROLES=warrior,assassin", request.Uri.ToString());
		}
	}

	public class ClientFactory_ConfigureFormatters
	{
		[Fact]
		public void ShouldSetClientFormatters()
		{
			var fluentHttpClientFactory = GetNewClientFactory();
			var clientBuilder = fluentHttpClientFactory.CreateBuilder("abc")
				.WithBaseUrl("http://abc.com")
				.ConfigureFormatters(opts =>
				{
					opts.Formatters.Clear();
				});

			var httpClient = fluentHttpClientFactory.Add(clientBuilder);

			Assert.Empty(httpClient.Formatters);
		}

		[Fact]
		public void ShouldSetDefaultFormatter()
		{
			var fluentHttpClientFactory = GetNewClientFactory();
			var clientBuilder = fluentHttpClientFactory.CreateBuilder("abc")
				.WithBaseUrl("http://abc.com")
				.ConfigureFormatters(opts =>
				{
					opts.Default = opts.Formatters.XmlFormatter;
				});

			var httpClient = fluentHttpClientFactory.Add(clientBuilder);

			Assert.Equal(httpClient.Formatters.XmlFormatter, httpClient.DefaultFormatter);
		}

		[Fact]
		public void ShouldAutoRegisterDefault()
		{
			var jsonFormatter = new JsonMediaTypeFormatter();
			var fluentHttpClientFactory = GetNewClientFactory();
			var clientBuilder = fluentHttpClientFactory.CreateBuilder("abc")
				.WithBaseUrl("http://abc.com")
				.ConfigureFormatters(opts =>
				{
					opts.Formatters.Clear();
					opts.Default = jsonFormatter;
				});

			var httpClient = fluentHttpClientFactory.Add(clientBuilder);

			Assert.Equal(jsonFormatter, httpClient.DefaultFormatter);
		}

		[Fact]
		public void DefaultFormatterShouldBePlacedFirst()
		{
			var fluentHttpClientFactory = GetNewClientFactory();
			var clientBuilder = fluentHttpClientFactory.CreateBuilder("abc")
				.WithBaseUrl("http://abc.com")
				.ConfigureFormatters(opts =>
				{
					opts.Default = opts.Formatters.XmlFormatter;
				});

			var httpClient = fluentHttpClientFactory.Add(clientBuilder);
			Assert.Equal(httpClient.Formatters.First(), httpClient.DefaultFormatter);
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