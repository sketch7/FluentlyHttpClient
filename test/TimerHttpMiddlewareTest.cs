using System;
using FluentlyHttpClient;
using FluentlyHttpClient.Middleware;
using FluentlyHttpClient.Test;
using RichardSzalay.MockHttp;
using Xunit;
using static FluentlyHttpClient.Test.ServiceTestUtil;

namespace Test
{
	public class TimerHttpMiddlewareTest
	{
		[Fact]
		public async void ShouldReturnContent()
		{
			var mockHttp = new MockHttpMessageHandler();
			mockHttp.When("http://sketch7.com/api/org/sketch7")
				.Respond("application/json", "{ 'name': 'sketch7' }");

			var fluentHttpClientFactory = GetNewClientFactory();
			fluentHttpClientFactory.CreateBuilder("sketch7")
				.WithBaseUrl("http://sketch7.com")
				.AddMiddleware<TimerHttpMiddleware>()
				.WithMessageHandler(mockHttp)
				.Register();

			var httpClient = fluentHttpClientFactory.Get("sketch7");
			var response = await httpClient.CreateRequest("/api/org/sketch7")
				.ReturnAsResponse<OrganizationModel>();

			Assert.NotNull(response.Data);
			Assert.Equal("sketch7", response.Data.Name);
			Assert.NotEqual(TimeSpan.Zero, response.GetTimeTaken());
		}
	}
}