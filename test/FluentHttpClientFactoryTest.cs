using System.Net.Http.Formatting;
using static FluentlyHttpClient.Test.ServiceTestUtil;

// ReSharper disable once CheckNamespace
namespace Test;

public class ClientFactoryTest_Build
{
	[Fact]
	public void ShouldAllowEmptyBaseUrl()
	{
		var httpClient = GetNewClientFactory().CreateBuilder("abc")
			.Build();

		Assert.Null(httpClient.BaseUrl);
	}
}

public class ClientFactory_WithRequestBuilderDefaults
{
	[Fact]
	public void ShouldHaveWithCustomDefaultsSet()
	{
		var httpClient = GetNewClientFactory().CreateBuilder("abc")
			.WithBaseUrl("http://abc.com")
			.WithRequestBuilderDefaults(builder => builder.AsPut())
			.Build();

		var request = httpClient.CreateRequest("/api")
			.Build();

		Assert.NotNull(request);
		Assert.Equal(HttpMethod.Put, request.Method);
	}

	[Fact]
	public void ShouldHaveCustomDefaultsCombined()
	{
		var httpClient = GetNewClientFactory().CreateBuilder("abc")
			.WithBaseUrl("http://abc.com")
			.WithRequestBuilderDefaults(builder => builder.AsPut())
			.WithRequestBuilderDefaults(builder => builder.WithItem("context", "user"))
			.Build();

		var request = httpClient.CreateRequest("/api")
			.Build();

		Assert.NotNull(request);
		Assert.Equal(HttpMethod.Put, request.Method);
		Assert.Equal("user", request.Items["context"]);
	}

	[Fact]
	public void ShouldHavePreviousCustomDefaultsReplaced()
	{
		var httpClient = GetNewClientFactory().CreateBuilder("abc")
			.WithBaseUrl("http://abc.com")
			.WithRequestBuilderDefaults(builder => builder.AsPut())
			.WithRequestBuilderDefaults(builder => builder.WithItem("context", "user"), replace: true)
			.Build();

		var request = httpClient.CreateRequest("/api")
			.Build();

		Assert.NotNull(request);
		Assert.Equal(HttpMethod.Get, request.Method);
		Assert.Equal("user", request.Items["context"]);
	}

	[Fact]
	public void ShouldHaveQueryParamsDefaultsSet()
	{
		var httpClient = GetNewClientFactory().CreateBuilder("abc")
			.WithBaseUrl("http://abc.com")
			.WithRequestBuilderDefaults(builder =>
				builder.WithQueryParamsOptions(opts =>
				{
					opts.CollectionMode = QueryStringCollectionMode.CommaSeparated;
					opts.WithKeyFormatter(key => key.ToUpper());
				})
			)
			.Build();

		var request = httpClient.CreateRequest("/api/heroes")
			.WithQueryParams(new
			{
				Roles = new List<string> { "warrior", "assassin" },
			})
			.Build();

		Assert.Equal("/api/heroes?ROLES=warrior,assassin", request.Uri.ToString());
	}

	[Fact]
	public void ShouldAppendToParentsBaseUrl()
	{
		var httpClient = GetNewClientFactory()
				.CreateBuilder("abc")
				.WithBaseUrl("http://abc.com")
				.Build()
			;

		var subHttpClient = httpClient
				.CreateClient("sub")
				.WithBaseUrl("/v1", replace: false)
				.Build()
			;

		Assert.Equal("http://abc.com/v1/", subHttpClient.BaseUrl);
		Assert.Equal("http://abc.com/", httpClient.BaseUrl);
	}
}

public class ClientFactory_ConfigureFormatters
{
	[Fact]
	public void ShouldSetClientFormatters()
	{
		var httpClient = GetNewClientFactory().CreateBuilder("abc")
			.WithBaseUrl("http://abc.com")
			.ConfigureFormatters(opts => { opts.Formatters.Clear(); })
			.Build();

		Assert.Empty(httpClient.Formatters);
	}

	[Fact]
	public void ShouldSetDefaultFormatter()
	{
		var httpClient = GetNewClientFactory().CreateBuilder("abc")
			.WithBaseUrl("http://abc.com")
			.ConfigureFormatters(opts =>
			{
				opts.Default = opts.Formatters.XmlFormatter;
			})
			.Build();

		Assert.Equal(httpClient.Formatters.XmlFormatter, httpClient.DefaultFormatter);
	}

	[Fact]
	public void SetDefaultFormatterMany_ShouldBeSetCorrectly()
	{
		var clientBuilder = GetNewClientFactory()
				.CreateBuilder("abc")
				.WithBaseUrl("http://abc.com")
			;
		var httpClient = clientBuilder
				.ConfigureFormatters(opts => opts.Default = opts.Formatters.XmlFormatter)
				.Build()
			;
		var httpClient2 = clientBuilder
				.ConfigureFormatters(opts => opts.Default = opts.Formatters.FormUrlEncodedFormatter)
				.Build()
			;

		Assert.Equal(httpClient.Formatters.XmlFormatter, httpClient.DefaultFormatter);
		Assert.Equal(httpClient2.Formatters.FormUrlEncodedFormatter, httpClient2.DefaultFormatter);
	}

	[Fact]
	public void ShouldAutoRegisterDefault()
	{
		var jsonFormatter = new JsonMediaTypeFormatter();
		var httpClient = GetNewClientFactory().CreateBuilder("abc")
			.WithBaseUrl("http://abc.com")
			.ConfigureFormatters(opts =>
			{
				opts.Formatters.Clear();
				opts.Default = jsonFormatter;
			})
			.Build();

		Assert.Equal(jsonFormatter, httpClient.DefaultFormatter);
	}

	[Fact]
	public void DefaultFormatterShouldBePlacedFirst()
	{
		var httpClient = GetNewClientFactory().CreateBuilder("abc")
			.WithBaseUrl("http://abc.com")
			.ConfigureFormatters(opts =>
			{
				opts.Default = opts.Formatters.XmlFormatter;
			})
			.Build();

		Assert.Equal(httpClient.Formatters.First(), httpClient.DefaultFormatter);
	}
}

public class ClientFactory_WithConfigureDefaults
{
	[Fact]
	public void ShouldSetClientFormatters()
	{
		var fluentHttpClientFactory = GetNewClientFactory();
		var httpClient = fluentHttpClientFactory.ConfigureDefaults(builder => builder.WithUserAgent("hots"))
			.CreateBuilder("abc")
			.WithBaseUrl("http://abc.com")
			.Build();

		var userAgentHeader = httpClient.Headers.GetValues("User-Agent").FirstOrDefault();
		Assert.Equal("hots", userAgentHeader);
	}
}

public class ClientFactory_Register
{
	[Fact]
	public void ShouldRegisterSuccessfully()
	{
		var httpClient = GetNewClientFactory().CreateBuilder("abc")
			.WithBaseUrl("http://abc.com")
			.Build();

		Assert.NotNull(httpClient);
		Assert.Equal("abc", httpClient.Identifier);
	}

	[Fact]
	public void ThrowsErrorWhenIdentifierNotSpecified()
	{
		var clientBuilder = GetNewClientFactory().CreateBuilder(null!);
		Assert.Throws<ClientBuilderValidationException>(() => clientBuilder.Register());
	}

	[Fact]
	public void ThrowsErrorWhenAlreadyRegistered()
	{
		var clientBuilder = GetNewClientFactory().CreateBuilder("abc")
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