using Microsoft.Extensions.DependencyInjection;
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

		httpClient.BaseUrl.ShouldBeNull();
	}
}

public class ClientFactory_WithRequestBuilderDefaults
{
	[Fact]
	public void AddFluentlyHttpClient_Defaults_ShouldBeSet()
	{
		var f = new ServiceCollection()
			.AddFluentlyHttpClient(defaults => defaults
				.WithTimeout(200)
				.WithUserAgent("default-config")
			).BuildServiceProvider()
			.GetRequiredService<IFluentHttpClientFactory>();

		var client = f.CreateBuilder("sketch7").Build();
		client.Headers.UserAgent.ToString().ShouldBe("default-config");
	}


	[Fact]
	public void Build_RegisterMulti_ShouldNotReplacePrevious()
	{
		var builder = GetNewClientFactory().CreateBuilder("A")
			.WithBaseUrl("http://abc.com")
			.WithHeader("X-S7", "a")
			.WithHeader("X-Org", "s7")
			.WithBasicAuthentication("user", "pa$$");

		var httpClientA = builder.Build();

		var httpClientB = builder
			.WithHeader("X-S7", "b")
			.WithIdentifier("B")
			.WithBasicAuthentication("user-2", "pa$$")
			.Build();

		httpClientA.Headers.TryGetValues("X-Org", out var orgHeadersA);
		httpClientA.Headers.TryGetValues("X-S7", out var s7HeadersA);

		httpClientB.Headers.TryGetValues("X-S7", out var s7HeadersB);
		httpClientA.Headers.TryGetValues("X-Org", out var orgHeadersB);

		orgHeadersA.ShouldNotBeNull().ShouldHaveSingleItem().ShouldBe("s7");
		s7HeadersA.ShouldNotBeNull().ShouldHaveSingleItem().ShouldBe("a");
		httpClientA.Headers.Authorization!.Parameter.ShouldBe("dXNlcjpwYSQk");

		orgHeadersB.ShouldNotBeNull().ShouldHaveSingleItem().ShouldBe("s7");
		s7HeadersB.ShouldNotBeNull().ShouldHaveSingleItem().ShouldBe("b");
		httpClientB.Headers.Authorization!.Parameter.ShouldBe("dXNlci0yOnBhJCQ=");
	}

	[Fact]
	public void ShouldHaveWithCustomDefaultsSet()
	{
		var httpClient = GetNewClientFactory().CreateBuilder("abc")
			.WithBaseUrl("http://abc.com")
			.WithRequestBuilderDefaults(builder => builder.AsPut())
			.Build();

		var request = httpClient.CreateRequest("/api")
			.Build();

		request.ShouldNotBeNull();
		request.Method.ShouldBe(HttpMethod.Put);
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

		request.ShouldNotBeNull();
		request.Method.ShouldBe(HttpMethod.Put);
		request.Items["context"].ShouldBe("user");
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

		request.ShouldNotBeNull();
		request.Method.ShouldBe(HttpMethod.Get);
		request.Items["context"].ShouldBe("user");
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

		request.Uri?.ToString().ShouldBe("/api/heroes?ROLES=warrior,assassin");
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

		subHttpClient.BaseUrl.ShouldBe("http://abc.com/v1/");
		httpClient.BaseUrl.ShouldBe("http://abc.com/");
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

		httpClient.Formatters.ShouldBeEmpty();
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

		httpClient.DefaultFormatter.ShouldBe(httpClient.Formatters.XmlFormatter);
	}

	[Fact]
	public void SetDefaultFormatterMany_ShouldBeSetCorrectly()
	{
		var clientBuilder = GetNewClientFactory()
				.ConfigureDefaults(x => x.WithAutoRegisterFactory(false))
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

		httpClient.DefaultFormatter.ShouldBe(httpClient.Formatters.XmlFormatter);
		httpClient2.DefaultFormatter.ShouldBe(httpClient2.Formatters.FormUrlEncodedFormatter);
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

		httpClient.DefaultFormatter.ShouldBe(jsonFormatter);
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

		httpClient.DefaultFormatter.ShouldBe(httpClient.Formatters.First());
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

		httpClient.Headers.GetValues("User-Agent").FirstOrDefault().ShouldBe("hots");
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

		httpClient.ShouldNotBeNull();
		httpClient.Identifier.ShouldBe("abc");
	}

	[Fact]
	public void ThrowsErrorWhenIdentifierNotSpecified()
	{
		var clientBuilder = GetNewClientFactory().CreateBuilder(null!);
		Should.Throw<ClientBuilderValidationException>(() => clientBuilder.Register());
	}

	[Fact]
	public void ThrowsErrorWhenAlreadyRegistered()
	{
		var clientBuilder = GetNewClientFactory().CreateBuilder("abc")
			.WithBaseUrl("http://abc.com")
			.Register();

		Should.Throw<ClientBuilderValidationException>(() => clientBuilder.Register());
	}
}

public class ClientFactory_Remove
{
	[Fact]
	public async Task ShouldDisposeClient()
	{
		var fluentHttpClientFactory = GetNewClientFactory();
		var clientBuilder = fluentHttpClientFactory.CreateBuilder("abc")
			.WithBaseUrl("http://abc.com");

		var httpClient = fluentHttpClientFactory.Add(clientBuilder);
		var isRegistered = fluentHttpClientFactory.Remove("abc")
			.Has("abc");

		await Should.ThrowAsync<ObjectDisposedException>(() => httpClient.Get<Hero>("/api/heroes/azmodan"));
		isRegistered.ShouldBeFalse();
	}
}