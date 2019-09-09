using FluentlyHttpClient.Middleware;
using Microsoft.Extensions.DependencyInjection;
using RichardSzalay.MockHttp;
using Serilog;
using System;
using System.Net.Http;
using Xunit;

namespace FluentlyHttpClient.Test.Integration
{
	public class SerilogIntegrationTest
	{
		private static IServiceProvider BuildContainer()
		{
			Log.Logger = new LoggerConfiguration()
				.WriteTo.Console()
				.WriteTo.Debug()
				.CreateLogger();
			var container = new ServiceCollection()
				.AddFluentlyHttpClient()
				.AddLogging(x => x.AddSerilog());
			return container.BuildServiceProvider();
		}

		[Fact]
		public async void ShouldLogAll()
		{
			var mockHttp = new MockHttpMessageHandler();
			mockHttp.When(HttpMethod.Post, "https://sketch7.com/api/heroes")
				.Respond("application/json", "{ 'name': 'Azmodan', 'title': 'Lord of Sin' }");

			var fluentHttpClientFactory = BuildContainer()
				.GetRequiredService<IFluentHttpClientFactory>();

			var httpClient = fluentHttpClientFactory.CreateBuilder("sketch7")
				.WithBaseUrl("https://sketch7.com")
				.WithHeader("X-SSV-VERSION", "1.0.0")
				.WithHeader("locale", new[] { "en", "fr" })
				.UseLogging(new LoggerHttpMiddlewareOptions
				{
					ShouldLogDetailedResponse = false,
					ShouldLogDetailedRequest = false
				})
				.WithMessageHandler(mockHttp)
				.Build();

			var hero = await httpClient.CreateRequest("/api/heroes")
				.AsPost()
				.WithBody(new Hero
				{
					Key = "valeera",
					Name = "Valeera",
					Title = "Shadow of the Uncrowned"
				})
				.WithLoggingOptions(new LoggerHttpMiddlewareOptions
				{
					ShouldLogDetailedRequest = true,
					ShouldLogDetailedResponse = true
				})
				.Return<Hero>();

			Assert.NotNull(hero);
			Assert.Equal("Azmodan", hero.Name);
		}
	}
}
