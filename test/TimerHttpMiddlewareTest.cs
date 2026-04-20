using static FluentlyHttpClient.Test.ServiceTestUtil;

namespace Test;

public class TimerHttpMiddlewareTest
{
	[Fact]
	public async Task ShouldHaveTimeTaken()
	{
		var mockHttp = new MockHttpMessageHandler();
		mockHttp.When("https://sketch7.com/api/heroes/azmodan")
			.Respond("application/json", "{ 'name': 'Azmodan' }");

		var httpClient = GetNewClientFactory().CreateBuilder("sketch7")
			.WithBaseUrl("https://sketch7.com")
			.WithMessageHandler(mockHttp)
			.UseTimer()
			.Build();

		var response = await httpClient.CreateRequest("/api/heroes/azmodan")
			.ReturnAsResponse<Hero>();

		response.Data.ShouldNotBeNull();
		response.Data.Name.ShouldBe("Azmodan");
		response.GetTimeTaken().ShouldNotBe(TimeSpan.Zero);
	}

	[Fact]
	public async Task ShouldWorkWithRequestThresholdOption()
	{
		var mockHttp = new MockHttpMessageHandler();
		mockHttp.When("https://sketch7.com/api/heroes/azmodan")
			.Respond("application/json", "{ 'name': 'Azmodan' }");

		var httpClient = GetNewClientFactory().CreateBuilder("sketch7")
			.WithBaseUrl("https://sketch7.com")
			.WithMessageHandler(mockHttp)
			.UseTimer()
			.Build();

		var response = await httpClient.CreateRequest("/api/heroes/azmodan")
			.WithTimerWarnThreshold(TimeSpan.FromSeconds(1))
			.ReturnAsResponse<Hero>();

		response.Data.ShouldNotBeNull();
		response.GetTimeTaken().ShouldNotBe(TimeSpan.Zero);
	}

	[Fact]
	public void ThrowsWhenWarnThresholdIsZero()
	{
		var httpClientBuilder = GetNewClientFactory().CreateBuilder("sketch7")
			.WithBaseUrl("https://sketch7.com")
			.UseTimer(x =>
			{
				x.WarnThreshold = TimeSpan.Zero;
			});

		Should.Throw<ArgumentException>(() => httpClientBuilder.Build());
	}
}