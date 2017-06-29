using FluentlyHttpClient.Middleware;
using RichardSzalay.MockHttp;
using System;
using System.Threading.Tasks;
using Xunit;
using static FluentlyHttpClient.Test.ServiceTestUtil;

namespace FluentlyHttpClient.Test
{
	public class TestHttpMiddleware : IFluentHttpMiddleware
	{
		private readonly FluentHttpRequestDelegate _next;

		public TestHttpMiddleware(FluentHttpRequestDelegate next)
		{
			_next = next;
		}

		public async Task<FluentHttpResponse> Invoke(FluentHttpRequest request)
		{
			request.Items["request"] = "item";
			var response = await _next(request);
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