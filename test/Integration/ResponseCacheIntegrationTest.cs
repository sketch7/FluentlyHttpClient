using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Xunit;

namespace FluentlyHttpClient.Test.Integration
{
	public class ResponseCacheIntegrationTest
	{
		private static void ConfigureContainer(IServiceCollection container)
		{
			Log.Logger = new LoggerConfiguration()
				.WriteTo.Console()
				.WriteTo.Debug()
				.CreateLogger();

			container.AddLogging(x => x.AddSerilog())
				;
		}

		//[Fact]
		public async Task ShouldMakeRequest_Memory_Get()
		{
			var fluentHttpClientFactory = ServiceTestUtil.GetNewClientFactory(ConfigureContainer);
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

			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			Assert.Equal("azmodan", response.Data.Key);

			response = await httpClient.CreateRequest("/api/heroes/azmodan")
				.ReturnAsResponse<Hero>();

			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			Assert.Equal("azmodan", response.Data.Key);

			response = await httpClient.CreateRequest("/api/heroes/azmodan")
				.ReturnAsResponse<Hero>();

			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			Assert.Equal("azmodan", response.Data.Key);
			Assert.Equal("Azmodan", response.Data.Name);
			Assert.Equal("Lord of Sin", response.Data.Title);
			Assert.Equal("Kestrel", response.Headers.Server.ToString());
			Assert.Equal(responseReason, response.ReasonPhrase);

			//Assert.Equal(HttpStatusCode.OK, response.Headers.);
		}

		//[Fact]
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

			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			Assert.Equal("azmodan", response.Data.Key);

			response = await httpClient.CreateRequest("/api/heroes/azmodan")
				.ReturnAsResponse<Hero>();

			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			Assert.Equal("azmodan", response.Data.Key);

			response = await httpClient.CreateRequest("/api/heroes/azmodan")
				.ReturnAsResponse<Hero>();

			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			Assert.Equal("azmodan", response.Data.Key);
			Assert.Equal("Azmodan", response.Data.Name);
			Assert.Equal("Lord of Sin", response.Data.Title);
			Assert.Equal("Kestrel", response.Headers.Server.ToString());
			Assert.Equal(responseReason, response.ReasonPhrase);

			//Assert.Equal(HttpStatusCode.OK, response.Headers.);
		}

		// [Fact]
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
}
