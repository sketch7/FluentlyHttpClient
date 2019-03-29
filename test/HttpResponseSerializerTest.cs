using System.Linq;
using System.Net;
using System.Net.Http;
using FluentlyHttpClient.Caching;
using RichardSzalay.MockHttp;
using Xunit;

namespace FluentlyHttpClient.Test
{
	public class HttpResponseSerializerTest
	{
		//[Fact]
		public async void ShouldBeSerialized()
		{
			var mockHttp = new MockHttpMessageHandler();
			mockHttp.When(HttpMethod.Post, "http://local.sketch7.io:5000/api/heroes/azmodan")
				.Respond("application/json", "{ 'name': 'Azmodan', 'title': 'Lord of Sin' }");

			var fluentHttpClientFactory = ServiceTestUtil.GetNewClientFactory();
			fluentHttpClientFactory.CreateBuilder("sketch7")
				.WithBaseUrl("http://local.sketch7.io:5000")
				.Register();

			var httpClient = fluentHttpClientFactory.Get("sketch7");
			var requestBuilder = httpClient.CreateRequest("/api/heroes/azmodan");
			var response = await requestBuilder.ReturnAsResponse();

			var serializer = new HttpResponseSerializer();
			var message = await serializer.Serialize<HttpResponseStore>(response);

			Assert.Equal("http://local.sketch7.io:5000/api/heroes/azmodan", message.Url);

			var response2 = await serializer.Deserialize(message);
			var hero = await response2.Content.ReadAsAsync<Hero>();
			Assert.NotNull(hero);
			Assert.Equal("Lord of Sin", hero.Title);
			Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
			Assert.Equal(response.Headers.Count(), response2.Headers.Count());
			Assert.Equal(response.Message.RequestMessage.RequestUri.ToString(), response2.Message.RequestMessage.RequestUri.ToString());
		}
	}
}