using FluentlyHttpClient;
using FluentlyHttpClient.Test;
using RichardSzalay.MockHttp;
using Xunit;
using static FluentlyHttpClient.Test.ServiceTestUtil;

namespace Test
{
	public class HttpClient_Get
	{
		[Fact]
		public async void ShouldReturnContent()
		{
			var mockHttp = new MockHttpMessageHandler();
			mockHttp.When("http://sketch7.com/api/org/sketch7")
				.Respond("application/json", "{ 'name': 'sketch7' }");

			var fluentHttpClientFactory = GetNewClientFactory();
			fluentHttpClientFactory.CreateBuilder("sketch7")
				.WithBaseUrl("http://sketch7.com")
				.WithMessageHandler(mockHttp)
				.Register();

			var httpClient = fluentHttpClientFactory.Get("sketch7");
			var organization = await httpClient.Get<OrganizationModel>("/api/org/sketch7");

			Assert.NotNull(organization);
			Assert.Equal("sketch7", organization.Name);
		}
	}
}