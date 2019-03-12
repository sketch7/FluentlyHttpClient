using System.Net;
using MessagePack.Resolvers;
using Sketch7.MessagePack.MediaTypeFormatter;
using Xunit;
using static FluentlyHttpClient.Test.ServiceTestUtil;

namespace FluentlyHttpClient.Test.Integration
{
	public class MessagePackIntegrationTest
	{
		private readonly MessagePackMediaTypeFormatter _messagePackMediaTypeFormatter = new MessagePackMediaTypeFormatter(ContractlessStandardResolver.Instance);

		// [Fact]
		public async void ShouldMakeRequest_Get()
		{
			var httpClient = GetNewClientFactory().CreateBuilder("sketch7")
				.WithBaseUrl("http://localhost:5001")
				.UseTimer()
				.ConfigureFormatters(opts =>
				{
					opts.Default = _messagePackMediaTypeFormatter;
				})
				.Build();

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
			var httpClient = GetNewClientFactory().CreateBuilder("sketch7")
				.WithBaseUrl("http://localhost:5001")
				.ConfigureFormatters(opts =>
					{
						opts.Default = _messagePackMediaTypeFormatter;
					})
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
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			Assert.Equal("valeera", response.Data.Key);
			Assert.Equal("Valeera", response.Data.Name);
			Assert.Equal("Shadow of the Uncrowned", response.Data.Title);
		}
	}
}
