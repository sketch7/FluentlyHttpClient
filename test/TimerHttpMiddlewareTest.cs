using System;
using FluentlyHttpClient;
using FluentlyHttpClient.Test;
using RichardSzalay.MockHttp;
using Xunit;
using static FluentlyHttpClient.Test.ServiceTestUtil;

namespace Test
{
	public class TimerHttpMiddlewareTest
	{
		[Fact]
		public async void ShouldHaveTimeTaken()
		{
			var mockHttp = new MockHttpMessageHandler();
			mockHttp.When("https://sketch7.com/api/heroes/azmodan")
				.Respond("application/json", "{ 'name': 'Azmodan' }");

			var fluentHttpClientFactory = GetNewClientFactory();
			var clientBuilder = fluentHttpClientFactory.CreateBuilder("sketch7")
				.WithBaseUrl("https://sketch7.com")
				.WithMessageHandler(mockHttp)
				.UseTimer();

			var httpClient = fluentHttpClientFactory.Add(clientBuilder);
			var response = await httpClient.CreateRequest("/api/heroes/azmodan")
				.ReturnAsResponse<Hero>();

			Assert.NotNull(response.Data);
			Assert.Equal("Azmodan", response.Data.Name);
			Assert.NotEqual(TimeSpan.Zero, response.GetTimeTaken());
		}

		[Fact]
		public async void ShouldWorkWithRequestThresholdOption()
		{
			var mockHttp = new MockHttpMessageHandler();
			mockHttp.When("https://sketch7.com/api/heroes/azmodan")
				.Respond("application/json", "{ 'name': 'Azmodan' }");

			var fluentHttpClientFactory = GetNewClientFactory();
			var clientBuilder = fluentHttpClientFactory.CreateBuilder("sketch7")
				.WithBaseUrl("https://sketch7.com")
				.WithMessageHandler(mockHttp)
				.UseTimer();

			var httpClient = fluentHttpClientFactory.Add(clientBuilder);
			var response = await httpClient.CreateRequest("/api/heroes/azmodan")
				.WithTimerWarnThreshold(TimeSpan.FromSeconds(1))
				.ReturnAsResponse<Hero>();

			Assert.NotNull(response.Data);
			Assert.NotEqual(TimeSpan.Zero, response.GetTimeTaken());
		}

		[Fact]
		public async void ThrowsWhenWarnThresholdIsZero()
		{
			var fluentHttpClientFactory = GetNewClientFactory();
			var clientBuilder = fluentHttpClientFactory.CreateBuilder("sketch7")
				.WithBaseUrl("https://sketch7.com")
				.UseTimer(x =>
				{
					x.WarnThreshold = TimeSpan.Zero;
				});

			var httpClient = fluentHttpClientFactory.Add(clientBuilder);
			await Assert.ThrowsAsync<ArgumentException>(() => httpClient.Get<Hero>("/api/heroes/azmodan"));
		}
	}
}