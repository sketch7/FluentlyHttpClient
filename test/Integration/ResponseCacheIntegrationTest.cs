using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Net;

namespace FluentlyHttpClient.Test.Integration;

public class ResponseCacheIntegrationTest(SampleApiFactory factory) : IClassFixture<SampleApiFactory>
{
	private static void ConfigureContainer(IServiceCollection container)
	{
		Log.Logger = new LoggerConfiguration()
			.WriteTo.Console()
			.WriteTo.Debug()
			.CreateLogger();

		container.AddLogging(x => x.AddSerilog());
	}

	[Fact]
	public async Task ShouldMakeRequest_Memory_Get()
	{
		var fluentHttpClientFactory = ServiceTestUtil.GetNewClientFactory(ConfigureContainer);
		var clientBuilder = fluentHttpClientFactory.CreateBuilder("sketch7")
				.WithBaseUrl("http://localhost")
				.WithMessageHandler(factory.Server.CreateHandler())
				.WithHeader("locale", "en-GB")
				.WithHeader("X-SSV-VERSION", "2019.02-2")
				.UseResponseCaching()
				.UseTimer()
			;
		var httpClient = fluentHttpClientFactory.Add(clientBuilder);
		var response = await httpClient.CreateRequest("/api/heroes/azmodan")
			.WithBearerAuthentication("XXX")
			.ReturnAsResponse<Hero>();

		var responseReason = response.ReasonPhrase;

		response.StatusCode.ShouldBe(HttpStatusCode.OK);
		response.Data!.Key.ShouldBe("azmodan");

		response = await httpClient.CreateRequest("/api/heroes/azmodan")
			.ReturnAsResponse<Hero>();

		response.StatusCode.ShouldBe(HttpStatusCode.OK);
		response.Data!.Key.ShouldBe("azmodan");

		response = await httpClient.CreateRequest("/api/heroes/azmodan")
			.ReturnAsResponse<Hero>();

		response.StatusCode.ShouldBe(HttpStatusCode.OK);
		response.Data!.Key.ShouldBe("azmodan");
		response.Data!.Name.ShouldBe("Azmodan");
		response.Data!.Title.ShouldBe("Lord of Sin");
		response.ReasonPhrase.ShouldBe(responseReason);
	}

	[Fact]
	[Trait("Category", "e2e")]
	public async Task ShouldMakeRequest_Remote_Get()
	{
		var fluentHttpClientFactory = ServiceTestUtil.GetNewClientFactory(collection =>
		{
			ConfigureContainer(collection);
			collection.AddFluentlyHttpClientEntity(
				"Data Source=.\\SQLEXPRESS;Database=FluentHttpClient;Integrated Security=True");
		});

		var clientBuilder = fluentHttpClientFactory.CreateBuilder("sketch7")
				//.WithBaseUrl("https://localhost:5001")
				.WithBaseUrl("http://local.sketch7.io:5000")
				.WithHeader("locale", "en-GB")
				.WithHeader("X-SSV-VERSION", "2019.02-2")
				.UseResponseCaching()
				.UseTimer()
			;
		var httpClient = fluentHttpClientFactory.Add(clientBuilder);
		var response = await httpClient.CreateRequest("/api/heroes/azmodan")
			.WithBearerAuthentication("XXX")
			.ReturnAsResponse<Hero>();

		var responseReason = response.ReasonPhrase;

		response.StatusCode.ShouldBe(HttpStatusCode.OK);
		response.Data!.Key.ShouldBe("azmodan");

		response = await httpClient.CreateRequest("/api/heroes/azmodan")
			.ReturnAsResponse<Hero>();

		response.StatusCode.ShouldBe(HttpStatusCode.OK);
		response.Data!.Key.ShouldBe("azmodan");

		response = await httpClient.CreateRequest("/api/heroes/azmodan")
			.ReturnAsResponse<Hero>();

		response.StatusCode.ShouldBe(HttpStatusCode.OK);
		response.Data!.Key.ShouldBe("azmodan");
		response.Data!.Name.ShouldBe("Azmodan");
		response.Data!.Title.ShouldBe("Lord of Sin");
		response.Headers.Server.ToString().ShouldBe("Kestrel");
		response.ReasonPhrase.ShouldBe(responseReason);

		//Assert.Equal(HttpStatusCode.OK, response.Headers.);
	}

	//[Fact]
	//[Trait("Category", "e2e")]
	//public async Task ShouldMakeRequest_Post()
	//{
	//	var fluentHttpClientFactory = ServiceTestUtil.GetNewClientFactory();
	//	var clientBuilder = fluentHttpClientFactory.CreateBuilder("sketch7")
	//		.WithBaseUrl("http://localhost:5001")
	//		.ConfigureFormatters(opts =>
	//			{
	//				opts.Default = _messagePackMediaTypeFormatter;
	//			})
	//		;
	//	var httpClient = fluentHttpClientFactory.Add(clientBuilder);
	//	var response = await httpClient.CreateRequest("/api/heroes")
	//		.AsPost()
	//		.WithBody(new Hero
	//		{
	//			Key = "valeera",
	//			Name = "Valeera",
	//			Title = "Shadow of the Ucrowned"
	//		})
	//		.ReturnAsResponse<Hero>();
	//	Assert.Equal(HttpStatusCode.OK, response.StatusCode);
	//	Assert.Equal("valeera", response.Data.Key);
	//	Assert.Equal("Valeera", response.Data.Name);
	//	Assert.Equal("Shadow of the Ucrowned", response.Data.Title);
	//}
}