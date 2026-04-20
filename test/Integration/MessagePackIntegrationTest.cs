using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using static FluentlyHttpClient.Test.ServiceTestUtil;

namespace FluentlyHttpClient.Test.Integration;

public class MessagePackIntegrationTest(SampleApiFactory factory) : IClassFixture<SampleApiFactory>
{
	[Fact]
	public async Task ShouldMakeRequest_Get()
	{
		var httpClient = GetNewClientFactory().CreateBuilder("sketch7")
			.WithBaseUrl("http://localhost")
			.WithMessageHandler(factory.Server.CreateHandler())
			.UseTimer()
			.Build();

		var response = await httpClient.CreateRequest("/api/heroes/azmodan")
			.ReturnAsResponse<Hero>();

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		Assert.Equal("azmodan", response.Data!.Key);
		Assert.Equal("Azmodan", response.Data!.Name);
		Assert.Equal("Lord of Sin", response.Data!.Title);
	}

	[Fact]
	public async Task ShouldMakeRequest_Post()
	{
		var httpClient = GetNewClientFactory().CreateBuilder("sketch7")
			.WithBaseUrl("http://localhost")
			.WithMessageHandler(factory.Server.CreateHandler())
			.Build();

		var response = await httpClient.CreateRequest("/api/heroes")
			.AsPost()
			.WithBody(new Hero
			{
				Key = "valeera",
				Name = "Valeera",
				Title = "Shadow of the Uncrowned"
			})
			.ReturnAsResponse<Hero>();

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		Assert.Equal("valeera", response.Data!.Key);
		Assert.Equal("Valeera", response.Data!.Name);
		Assert.Equal("Shadow of the Uncrowned", response.Data!.Title);
	}
}