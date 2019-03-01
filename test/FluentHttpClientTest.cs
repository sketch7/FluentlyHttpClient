using System.Linq;
using System.Net.Http;
using FluentlyHttpClient;
using FluentlyHttpClient.GraphQL;
using FluentlyHttpClient.Test;
using RichardSzalay.MockHttp;
using Xunit;
using static FluentlyHttpClient.Test.ServiceTestUtil;

namespace Test
{
	public class HttpClient
	{
		[Fact]
		public async void Get_ShouldReturnContent()
		{
			var mockHttp = new MockHttpMessageHandler();
			mockHttp.When("https://sketch7.com/api/heroes/azmodan")
				.Respond("application/json", "{ 'name': 'Azmodan' }");

			var clientBuilder = GetNewClientFactory().CreateBuilder("sketch7")
				.WithBaseUrl("https://sketch7.com")
				.WithMessageHandler(mockHttp);

			var httpClient = clientBuilder.Build();
			var hero = await httpClient.Get<Hero>("/api/heroes/azmodan");

			Assert.NotNull(hero);
			Assert.Equal("Azmodan", hero.Name);
		}

		[Fact]
		public async void Post_ShouldReturnContent()
		{
			var mockHttp = new MockHttpMessageHandler();
			mockHttp.When(HttpMethod.Post, "https://sketch7.com/api/heroes/azmodan")
				//.WithContent("{\"Title\":\"Lord of Sin\"}")
				.With(request =>
				{
					var contentTask = request.Content.ReadAsAsync<Hero>();
					contentTask.Wait();
					return contentTask.Result.Title == "Lord of Sin";
				})
				.Respond("application/json", "{ 'name': 'Azmodan', 'title': 'Lord of Sin' }");

			var clientBuilder = GetNewClientFactory().CreateBuilder("sketch7")
				.WithBaseUrl("https://sketch7.com")
				.WithMessageHandler(mockHttp);

			var httpClient = clientBuilder.Build();
			var hero = await httpClient.Post<Hero>("/api/heroes/azmodan", new
			{
				Title = "Lord of Sin"
			});

			Assert.NotNull(hero);
			Assert.Equal("Lord of Sin", hero.Title);
		}

		[Fact]
		public void Create_ShouldReturnANewClient()
		{
			var clientBuilder = GetNewClientFactory().CreateBuilder("sketch7")
					.WithBaseUrl("https://sketch7.com")
					.WithHeader("X-SSV-Locale", "en-GB")
					.WithRequestBuilderDefaults(requestBuilder => requestBuilder.WithUri("api/graphql"))
				;

			var httpClient = clientBuilder.Build();
			var subClient = httpClient.CreateClient("subclient")
					.WithHeader("X-SSV-Locale", "de")
					.WithHeader("X-SSV-Country", "de")
					.Build()
				;

			var httpClientLocale = httpClient.Headers.GetValues("X-SSV-Locale").FirstOrDefault();
			var subClientLocale = subClient.Headers.GetValues("X-SSV-Locale").FirstOrDefault();

			httpClient.Headers.TryGetValues("X-SSV-Country", out var countryValues);
			var subClientCountry = subClient.Headers.GetValues("X-SSV-Country").FirstOrDefault();

			Assert.Equal("en-GB", httpClientLocale);
			Assert.Equal("de", subClientLocale);
			Assert.Null(countryValues?.FirstOrDefault());
			Assert.Equal("de", subClientCountry);
		}

		[Fact]
		public async void GraphQL_ShouldReturnContent()
		{
			const string query = "{hero {name,title}}";

			var mockHttp = new MockHttpMessageHandler();
			mockHttp.When(HttpMethod.Post, "https://sketch7.com/api/graphql")
				.With(request =>
				{
					var contentTask = request.Content.ReadAsAsync<GqlQuery>();
					contentTask.Wait();
					return contentTask.Result.Query == query;
				})
				.Respond("application/json", "{ 'data': {'name': 'Azmodan', 'title': 'Lord of Sin' }}");

			var clientBuilder = GetNewClientFactory().CreateBuilder("sketch7")
				.WithBaseUrl("https://sketch7.com")
				.WithRequestBuilderDefaults(requestBuilder => requestBuilder.WithUri("api/graphql"))
				.WithMessageHandler(mockHttp);

			var httpClient = clientBuilder.Build();
			var response = await httpClient.CreateGqlRequest(query)
				.ReturnAsGqlResponse<Hero>();

			Assert.True(response.IsSuccessStatusCode);
			Assert.NotNull(response.Data);
			Assert.Equal("Lord of Sin", response.Data.Title);
		}
	}
}