using FluentlyHttpClient;
using FluentlyHttpClient.Test;
using RichardSzalay.MockHttp;
using System.Net.Http;
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

			var fluentHttpClientFactory = GetNewClientFactory();
			fluentHttpClientFactory.CreateBuilder("sketch7")
				.WithBaseUrl("https://sketch7.com")
				.WithMessageHandler(mockHttp)
				.Register();

			var httpClient = fluentHttpClientFactory.Get("sketch7");
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

			var fluentHttpClientFactory = GetNewClientFactory();
			fluentHttpClientFactory.CreateBuilder("sketch7")
				.WithBaseUrl("https://sketch7.com")
				.WithMessageHandler(mockHttp)
				.Register();

			var httpClient = fluentHttpClientFactory.Get("sketch7");
			var hero = await httpClient.Post<Hero>("/api/heroes/azmodan", new
			{
				Title = "Lord of Sin"
			});

			Assert.NotNull(hero);
			Assert.Equal("Lord of Sin", hero.Title);
		}
	}

}