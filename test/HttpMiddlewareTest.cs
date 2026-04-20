using FluentlyHttpClient.Middleware;
using static FluentlyHttpClient.Test.ServiceTestUtil;

namespace FluentlyHttpClient.Test;

public class TestHttpMiddleware : IFluentHttpMiddleware
{
	private readonly FluentHttpMiddlewareDelegate _next;

	public TestHttpMiddleware(FluentHttpMiddlewareDelegate next, FluentHttpMiddlewareClientContext _context)
	{
		_next = next;
	}

	public async Task<FluentHttpResponse> Invoke(FluentHttpMiddlewareContext context)
	{
		context.Request.Items["request"] = "item";
		var response = await _next(context);
		return response;
	}
}

public class TestRawHttpMiddleware : IFluentHttpMiddleware
{
	private readonly FluentHttpMiddlewareDelegate _next;

	public TestRawHttpMiddleware(FluentHttpMiddlewareDelegate next, FluentHttpMiddlewareClientContext _context)
	{
		_next = next;
	}

	public async Task<FluentHttpResponse> Invoke(FluentHttpMiddlewareContext context)
	{
		var response = await _next(context);
		response.Headers.Add("X-Brand-Id", "snorlax");

		if (response.Items?.Count > 0)
		{
			foreach (var (key, value) in response.Items)
			{
				response.Headers.Add(key?.ToString() ?? string.Empty, value?.ToString());
			}
		}

		return response;
	}
}

public class HttpMiddlewareTest
{
	[Fact]
	public async Task ShouldHaveRequestItem()
	{
		var mockHttp = new MockHttpMessageHandler();
		mockHttp.When("https://sketch7.com/api/heroes/azmodan")
			.Respond("application/json", "{ 'name': 'Azmodan' }");

		var fluentHttpClientFactory = GetNewClientFactory();
		fluentHttpClientFactory.CreateBuilder("sketch7")
			.WithBaseUrl("https://sketch7.com")
			.WithMessageHandler(mockHttp)
			.UseMiddleware<TestHttpMiddleware>()
			.Register();

		var httpClient = fluentHttpClientFactory.Get("sketch7");
		var response = await httpClient.CreateRequest("/api/heroes/azmodan")
			.WithItem("monster", "orsachiottolo")
			.ReturnAsResponse<Hero>();

		response.Items.TryGetValue("request", out var requestItem);
		response.Items.TryGetValue("monster", out var monsterItem);

		requestItem.ShouldBe("item");
		monsterItem.ShouldBe("orsachiottolo");
	}

	[Fact]
	public async Task ShouldHaveRequestItem_WhenRawRequestProps()
	{
		var mockHttp = new MockHttpMessageHandler();
		mockHttp.When("https://sketch7.com/api/heroes/azmodan")
			.Respond("application/json", "{ 'name': 'Azmodan' }");

		var fluentHttpClientFactory = GetNewClientFactory();
		fluentHttpClientFactory.CreateBuilder("sketch7")
			.WithBaseUrl("https://sketch7.com")
			.WithMessageHandler(mockHttp)
			.UseMiddleware<TestHttpMiddleware>()
			.Register();

		var httpClient = fluentHttpClientFactory.Get("sketch7");
		var request = httpClient.CreateRequest("/api/heroes/azmodan").Build().Message;
		request.Options.Set(new HttpRequestOptionsKey<string>("monster"), "orsachiottolo");

		var response = await httpClient.Send(request);

		response.Items.TryGetValue("request", out var requestItem);
		response.Items.TryGetValue("monster", out var monsterItem);

		requestItem.ShouldBe("item");
		monsterItem.ShouldBe("orsachiottolo");
	}

	[Fact]
	public async Task RawClient_ShouldGoThroughMiddleware()
	{
		var mockHttp = new MockHttpMessageHandler();
		mockHttp.When("https://sketch7.com/api/heroes/azmodan")
			.Respond("application/json", "{ 'name': 'Azmodan' }");

		var fluentHttpClientFactory = GetNewClientFactory();
		fluentHttpClientFactory.CreateBuilder("sketch7")
			.WithBaseUrl("https://sketch7.com")
			.WithMessageHandler(mockHttp)
			.UseMiddleware<TestHttpMiddleware>()
			.UseMiddleware<TestRawHttpMiddleware>()
			.Register();

		var httpClient = fluentHttpClientFactory.Get("sketch7");
		var response = await httpClient.RawHttpClient.GetAsync("/api/heroes/azmodan", TestContext.Current.CancellationToken);
		var brand = response.Headers.GetValues("X-Brand-Id");

		brand.First().ShouldBe("snorlax");
	}

	[Fact]
	public async Task RawClient_ShouldGoThroughMiddlewarePreservingItems()
	{
		var mockHttp = new MockHttpMessageHandler();
		mockHttp.When("https://sketch7.com/api/heroes/azmodan")
			.Respond("application/json", "{ 'name': 'Azmodan' }");

		var fluentHttpClientFactory = GetNewClientFactory();
		fluentHttpClientFactory.CreateBuilder("sketch7")
			.WithBaseUrl("https://sketch7.com")
			.WithMessageHandler(mockHttp)
			.UseMiddleware<TestRawHttpMiddleware>()
			.WithRequestBuilderDefaults(builder => builder.WithItem("monster", "orsachiottolo"))
			.Register();

		var httpClient = fluentHttpClientFactory.Get("sketch7");

		var response = await httpClient.RawHttpClient.GetAsync("/api/heroes/azmodan", TestContext.Current.CancellationToken);
		var brand = response.Headers.GetValues("X-Brand-Id");
		var monster = response.Headers.GetValues("monster");

		brand.First().ShouldBe("snorlax");
		monster.First().ShouldBe("orsachiottolo");
	}
}