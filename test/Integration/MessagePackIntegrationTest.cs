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

		response.StatusCode.ShouldBe(HttpStatusCode.OK);
		response.Data!.Key.ShouldBe("azmodan");
		response.Data!.Name.ShouldBe("Azmodan");
		response.Data!.Title.ShouldBe("Lord of Sin");
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

		response.StatusCode.ShouldBe(HttpStatusCode.OK);
		response.Data!.Key.ShouldBe("valeera");
		response.Data!.Name.ShouldBe("Valeera");
		response.Data!.Title.ShouldBe("Shadow of the Uncrowned");
	}
}