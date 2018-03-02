using System.Net;
using System.Net.Http;
using FluentlyHttpClient;
using FluentlyHttpClient.Middleware;
using FluentlyHttpClient.Test;
using RichardSzalay.MockHttp;
using Xunit;
using static FluentlyHttpClient.Test.ServiceTestUtil;

namespace Test
{
	public class LoggingHttpMiddlewareTest
	{
		[Fact]
		public async void RequestBodyWithoutContent_ShouldNotThrow()
		{
			var mockHttp = new MockHttpMessageHandler();
			mockHttp.When("https://sketch7.com/api/heroes/azmodan")
				.Respond("application/json", "{ 'name': 'Azmodan' }");

			var fluentHttpClientFactory = GetNewClientFactory();
			var clientBuilder = fluentHttpClientFactory.CreateBuilder("sketch7")
				.WithBaseUrl("https://sketch7.com")
				.UseLogging(new LoggerHttpMiddlewareOptions
				{
					ShouldLogDetailedRequest = true,
					ShouldLogDetailedResponse = true
				})
				.WithMessageHandler(mockHttp);

			var httpClient = fluentHttpClientFactory.Add(clientBuilder);
			var hero = await httpClient.Get<Hero>("/api/heroes/azmodan");

			Assert.NotNull(hero);
			Assert.Equal("Azmodan", hero.Name);
		}

		[Fact]
		public async void ResponseBodyWithoutContent_ShouldNotThrow()
		{
			var mockHttp = new MockHttpMessageHandler();
			mockHttp.When(HttpMethod.Post, "https://sketch7.com/api/heroes")
				.Respond("application/json", "");

			var fluentHttpClientFactory = GetNewClientFactory();
			var clientBuilder = fluentHttpClientFactory.CreateBuilder("sketch7")
				.WithBaseUrl("https://sketch7.com")
				.UseLogging(new LoggerHttpMiddlewareOptions
				{
					ShouldLogDetailedRequest = true,
					ShouldLogDetailedResponse = true
				})
				.WithMessageHandler(mockHttp);

			var httpClient = fluentHttpClientFactory.Add(clientBuilder);
			var response = await httpClient.CreateRequest("/api/heroes")
				.AsPost()
				.WithBody(new Hero
				{
					Key = "valeera",
					Name = "Valeera",
					Title = "Shadow of the Ucrowned"
				})
				.ReturnAsResponse();

			Assert.NotNull(response);
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		}
	}
}