using System.Diagnostics;
using System.Net;
using System.Net.Http;
using Xunit;
using static FluentlyHttpClient.Test.ServiceTestUtil;

namespace FluentlyHttpClient.Test.Integration
{
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class UploadResult
	{
		private string DebuggerDisplay => $"FileName: '{FileName}', ContentType: '{ContentType}', Size: {Size}";

		public string FileName { get; set; }
		public string ContentType { get; set; }
		public double Size { get; set; }
	}

	public class FileUploadIntegrationTest
	{
		[Fact]
		[Trait("Category", "e2e")]
		public async void Multipart_WithBodyContent()
		{
			var httpClient = GetNewClientFactory().CreateBuilder("sketch7")
				.WithBaseUrl("http://localhost:5500")
				.UseLogging(x => x.IsCondensed = true)
				.Build();

			var filePath = "./animal-mustache.jpg";

			var multiForm = new MultipartFormDataContent
			{
				{ "hero", "Jaina" }
			};
			await multiForm.AddFileAsync("file", filePath);

			//var r = await httpClient.RawHttpClient.PostAsync("/api/sample/upload", multiForm);

			var response = await httpClient.CreateRequest("/api/sample/upload")
				.AsPost()
				.WithBodyContent(multiForm)
				.ReturnAsResponse<UploadResult>();

			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			Assert.Equal("animal-mustache.jpg", response.Data.FileName);
			Assert.Equal("image/jpeg", response.Data.ContentType);
			Assert.Equal(3.96875, response.Data.Size);
		}
	}
}
