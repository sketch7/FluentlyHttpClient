using FluentlyHttpClient.Middleware;
using RichardSzalay.MockHttp;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using static FluentlyHttpClient.Test.ServiceTestUtil;

namespace FluentlyHttpClient.Test
{
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
					response.Headers.Add(key.ToString(), value.ToString());
				}
			}

			return response;
		}
	}

	public class HttpMiddlewareTest
	{
		[Fact]
		public async void ShouldHaveRequestItem()
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

			Assert.Equal("item", requestItem);
			Assert.Equal("orsachiottolo", monsterItem);
		}

		[Fact]
		public async void RawClient_ShouldGoThroughMiddleware()
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
			var response = await httpClient.RawHttpClient.GetAsync("/api/heroes/azmodan");
			var brand = response.Headers.GetValues("X-Brand-Id");

			Assert.Equal("snorlax", brand.First());
		}

		[Fact]
		public async void RawClient_ShouldGoThroughMiddlewarePreservingItems()
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

			var response = await httpClient.RawHttpClient.GetAsync("/api/heroes/azmodan");
			var brand = response.Headers.GetValues("X-Brand-Id");
			var monster = response.Headers.GetValues("monster");

			Assert.Equal("snorlax", brand.First());
			Assert.Equal("orsachiottolo", monster.First());
		}
	}
}