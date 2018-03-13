using System.Net;
using MessagePack.Resolvers;
using Sketch7.MessagePack.MediaTypeFormatter;
using Xunit;

namespace FluentlyHttpClient.Test.Integration
{
	public class MessagePackIntegrationTest
	{
		private readonly MessagePackMediaTypeFormatter _messagePackMediaTypeFormatter = new MessagePackMediaTypeFormatter(ContractlessStandardResolver.Instance);

		// [Fact]
		public async void ShouldMakeRequest_Get()
		{
			var fluentHttpClientFactory = ServiceTestUtil.GetNewClientFactory();
			var clientBuilder = fluentHttpClientFactory.CreateBuilder("sketch7")
				.WithBaseUrl("http://localhost:5001")
					.UseTimer()
				.ConfigureFormatters(opts =>
				{
					opts.Default = _messagePackMediaTypeFormatter;
				})
			;
			var httpClient = fluentHttpClientFactory.Add(clientBuilder);
			var response = await httpClient.CreateRequest("/api/heroes/azmodan")
				.ReturnAsResponse<Hero>();

			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			Assert.Equal("azmodan", response.Data.Key);
			Assert.Equal("Azmodan", response.Data.Name);
			Assert.Equal("Lord of Sins", response.Data.Title);
		}

		// [Fact]
		public async void ShouldMakeRequest_Post()
		{
			var fluentHttpClientFactory = ServiceTestUtil.GetNewClientFactory();
			var clientBuilder = fluentHttpClientFactory.CreateBuilder("sketch7")
				.WithBaseUrl("http://localhost:5001")
				.ConfigureFormatters(opts =>
					{
						opts.Default = _messagePackMediaTypeFormatter;
					})
				;
			var httpClient = fluentHttpClientFactory.Add(clientBuilder);
			var response = await httpClient.CreateRequest("/api/heroes")
				.AsPost()
				.WithBody(new Hero
				{
					Key = "valeera",
					Name = "Valeera",
					Title = "Shadow of the Ucrowned"
				})
				.ReturnAsResponse<Hero>();
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			Assert.Equal("valeera", response.Data.Key);
			Assert.Equal("Valeera", response.Data.Name);
			Assert.Equal("Shadow of the Ucrowned", response.Data.Title);
		}
	}
}
