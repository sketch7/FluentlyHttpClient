using RichardSzalay.MockHttp;
using Xunit;
using static FluentlyHttpClient.Test.ServiceTestUtil;

namespace Test
{
	public class OrganizationMock
	{
		public string Name { get; set; }
	}

	public class HttpClient_Get
	{
		[Fact]
		public async void ShouldReturnContent()
		{
			var mockHttp = new MockHttpMessageHandler();
			mockHttp.When("http://abc.com/api/org/sketch7")
				.Respond("application/json", "{ 'name': 'sketch7' }");

			var fluentHttpClientFactory = GetNewClientFactory();
			fluentHttpClientFactory.CreateBuilder("abc")
				.WithBaseUrl("http://abc.com")
				.WithMessageHandler(mockHttp)
				.Register();

			var httpClient = fluentHttpClientFactory.Get("abc");
			var organization = await httpClient.Get<OrganizationMock>("/api/org/sketch7");

			Assert.NotNull(organization);
			Assert.Equal("sketch7", organization.Name);
		}
	}
}