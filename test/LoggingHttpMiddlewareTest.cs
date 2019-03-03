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
			var httpClient = fluentHttpClientFactory.CreateBuilder("sketch7")
				.WithBaseUrl("https://sketch7.com")
				.UseLogging(new LoggerHttpMiddlewareOptions
				{
					ShouldLogDetailedRequest = true,
					ShouldLogDetailedResponse = true
				})
				.WithMessageHandler(mockHttp)
				.Build();

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

			var httpClient = GetNewClientFactory().CreateBuilder("sketch7")
				.WithBaseUrl("https://sketch7.com")
				.UseLogging(new LoggerHttpMiddlewareOptions
				{
					ShouldLogDetailedRequest = true,
					ShouldLogDetailedResponse = true
				})
				.WithMessageHandler(mockHttp)
				.Build();

			var response = await httpClient.CreateRequest("/api/heroes")
				.AsPost()
				.WithBody(new Hero
				{
					Key = "valeera",
					Name = "Valeera",
					Title = "Shadow of the Uncrowned"
				})
				.ReturnAsResponse();

			Assert.NotNull(response);
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		}

		[Fact]
		public async void UsingActionBasedConfiguration()
		{
			var mockHttp = new MockHttpMessageHandler();
			mockHttp.When(HttpMethod.Post, "https://sketch7.com/api/heroes")
				.Respond("application/json", "");

			var httpClient = GetNewClientFactory().CreateBuilder("sketch7")
				.WithBaseUrl("https://sketch7.com")
				.UseLogging(x =>
				{
					x.ShouldLogDetailedRequest = true;
					x.ShouldLogDetailedResponse = true;
				})
				.WithMessageHandler(mockHttp)
				.Build();

			var response = await httpClient.CreateRequest("/api/heroes")
				.AsPost()
				.WithBody(new Hero
				{
					Key = "valeera",
					Name = "Valeera",
					Title = "Shadow of the Uncrowned"
				})
				.ReturnAsResponse();

			Assert.NotNull(response);
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		}

		[Fact]
		public void DefaultLoggingOptions_ShouldBeMerged()
		{
			var loggerHttpMiddlewareOptions = new LoggerHttpMiddlewareOptions
			{
				ShouldLogDetailedRequest = true,
				ShouldLogDetailedResponse = true
			};
			var httpClient = GetNewClientFactory().CreateBuilder("sketch7")
				.WithBaseUrl("https://sketch7.com")
				.UseLogging(loggerHttpMiddlewareOptions)
				.Build();

			var request = httpClient.CreateRequest("/api/heroes")
				.AsPost()
				.Build();

			var options = request.GetLoggingOptions(loggerHttpMiddlewareOptions);

			Assert.True(options.ShouldLogDetailedRequest);
			Assert.True(options.ShouldLogDetailedResponse);
		}

		[Fact]
		public void RequestSpecificOptions_ShouldOverride()
		{
			var loggerHttpMiddlewareOptions = new LoggerHttpMiddlewareOptions
			{
				ShouldLogDetailedRequest = false,
				ShouldLogDetailedResponse = false
			};
			var httpClient = GetNewClientFactory().CreateBuilder("sketch7")
				.WithBaseUrl("https://sketch7.com")
				.UseLogging(loggerHttpMiddlewareOptions)
				.Build();

			var request = httpClient.CreateRequest("/api/heroes")
				.AsPost()
				.WithLoggingOptions(new LoggerHttpMiddlewareOptions
				{
					ShouldLogDetailedResponse = true,
					ShouldLogDetailedRequest = true
				})
				.Build();

			var options = request.GetLoggingOptions(loggerHttpMiddlewareOptions);

			Assert.True(options.ShouldLogDetailedRequest);
			Assert.True(options.ShouldLogDetailedResponse);
		}

		[Fact]
		public void RequestSpecificOptions_ActionBased()
		{
			var loggerHttpMiddlewareOptions = new LoggerHttpMiddlewareOptions
			{
				ShouldLogDetailedRequest = false,
				ShouldLogDetailedResponse = false
			};
			var httpClient = GetNewClientFactory().CreateBuilder("sketch7")
				.WithBaseUrl("https://sketch7.com")
				.UseLogging(loggerHttpMiddlewareOptions)
				.Build();

			var request = httpClient.CreateRequest("/api/heroes")
				.AsPost()
				.WithLoggingOptions(x =>
				{
					x.ShouldLogDetailedResponse = true;
					x.ShouldLogDetailedRequest = true;
				})
				.Build();

			var options = request.GetLoggingOptions(loggerHttpMiddlewareOptions);

			Assert.True(options.ShouldLogDetailedRequest);
			Assert.True(options.ShouldLogDetailedResponse);
		}
	}
}