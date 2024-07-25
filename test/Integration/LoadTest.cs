using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Net;

namespace FluentlyHttpClient.Test.Integration;

public record MimirGqlSchema
{
	public List<UniverseModel> UniversesIndex { get; set; }
}

public record UniverseModel
{
	public string Id { get; set; }
	public string Key { get; set; }
	public string Name { get; set; }
}

public class LoadTest
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
	[Trait("Category", "e2e")]
	public async void GqlHttp2Test()
	{
		var socketsHandler = new SocketsHttpHandler
		{
			PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
			KeepAlivePingDelay = TimeSpan.FromSeconds(60),
			KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
			EnableMultipleHttp2Connections = true,
		};
		var httpClient = BuildContainer()
			.GetRequiredService<IFluentHttpClientFactory>()
			.CreateBuilder("mimir")
			.WithBaseUrl("XXX/v1/api/graphql")
			.WithBaseUrlTrailingSlash(false)
			.UseLogging()
			.UseTimer()
			.ConfigureFormatters(opts =>
			{
				//opts.Default = opts.Formatters.SystemTextJsonFormatter();
			})
			//.WithRequestBuilderDefaults(x => x.WithVersion(HttpVersion.Version11))
			.WithMessageHandler(socketsHandler)
			.Build();

		for (int i = 0; i < 1; i++)
		{
			var tasks = Enumerable.Range(0, 1800)
				.Select(async (i) =>
					{
						var response = await httpClient.CreateGqlRequest(new()
						{
							//OperationName = "universe",
							//Variables =
							Query = @"
query universes_getByIndex($input: UniverseIndexQuery) {
	universesIndex(input: $input) {
		...Universe
	}
}

fragment Universe on Universe {
	id
	key
	name
	isArchived

	heroes @include(if: true) {
		id
		name
	}

}
										"
						})//CreateRequest("/api/heroes/azmodan")
							.ReturnAsGqlResponse<MimirGqlSchema>();
						//response.Message.Dispose();
						Assert.Equal(HttpStatusCode.OK, response.StatusCode);
						return response;
					}
				);
			await Task.WhenAll(tasks);
		}


		//var response = await httpClient.CreateRequest("/api/heroes/azmodan")
		//	.ReturnAsResponse<Hero>();
		////response.Message.Dispose();

		//Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		//Assert.Equal("azmodan", response.Data.Key);
		//Assert.Equal("Azmodan", response.Data.Name);
		//Assert.Equal("Lord of Sin", response.Data.Title);
	}
}