using System.Threading.Tasks;
using FluentlyHttpClient.Middleware;
using RichardSzalay.MockHttp;
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
				.ReturnAsResponse<Hero>();

			response.Items.TryGetValue("request", out var requestItem);

			Assert.Equal("item", requestItem);
		}
	}
}