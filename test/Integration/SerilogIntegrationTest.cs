using System;
using System.Net.Http;
using FluentlyHttpClient.Middleware;
using Microsoft.Extensions.DependencyInjection;
using RichardSzalay.MockHttp;
using Serilog;
using Xunit;

namespace FluentlyHttpClient.Test.Integration
{
	public class SerilogIntegrationTest
	{
		private IServiceProvider BuildContainer()
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

			var clientBuilder = fluentHttpClientFactory.CreateBuilder("sketch7")
				.WithBaseUrl("https://sketch7.com")
				.UseLogging(new LoggerHttpMiddlewareOptions
				{
					ShouldLogDetailedResponse = false,
					ShouldLogDetailedRequest = false
				})
				.WithMessageHandler(mockHttp);

			var httpClient = fluentHttpClientFactory.Add(clientBuilder);
			var hero = await httpClient.CreateRequest("/api/heroes")
				.AsPost()
				.WithBody(new Hero
				{
					Key = "valeera",
					Name = "Valeera",
					Title = "Shadow of the Ucrowned"
				})
				.WithLoggingOptions(new LoggerHttpMiddlewareOptions
				{
					ShouldLogDetailedRequest = false,
					ShouldLogDetailedResponse = true
				})
				.Return<Hero>();

			Assert.NotNull(hero);
			Assert.Equal("Azmodan", hero.Name);
		}
	}
}
