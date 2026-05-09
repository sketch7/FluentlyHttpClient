using System.Diagnostics;
using System.Net;
using static FluentlyHttpClient.Test.ServiceTestUtil;

namespace FluentlyHttpClient.Test.Integration;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class UploadResult
{
	private string DebuggerDisplay => $"FileName: '{FileName}', ContentType: '{ContentType}', Size: {Size}";

	public string FileName { get; set; } = null!;
	public string ContentType { get; set; } = null!;
	public double Size { get; set; }
}

public class FileUploadIntegrationTest(SampleApiFactory factory) : IClassFixture<SampleApiFactory>
{
	private IFluentHttpClient BuildClient()
		=> GetNewClientFactory().CreateBuilder("sketch7")
			.WithBaseUrl("http://localhost")
			.WithMessageHandler(factory.Server.CreateHandler())
			.UseLogging(x => x.IsCondensed = true)
			.Build();

	[Fact]
	public async Task MultipartForm_AddFile_Path()
	{
		var httpClient = BuildClient();
		var filePath = "./animal-mustache.jpg";

		var multiForm = new MultipartFormDataContent
		{
			{ "hero", "Jaina" }
		};
		multiForm.AddFile("file", filePath);

		var response = await httpClient.CreateRequest("/api/sample/upload")
			.AsPost()
			.WithBodyContent(multiForm)
			.ReturnAsResponse<UploadResult>();

		response.StatusCode.ShouldBe(HttpStatusCode.OK);
		response.Data!.FileName.ShouldBe("animal-mustache.jpg");
		response.Data!.ContentType.ShouldBe("image/jpeg");
		response.Data!.Size.ShouldBe(3.96875);
	}

	[Fact]
	public async Task MultipartForm_AddFile_FileStream()
	{
		var httpClient = BuildClient();
		var filePath = "./animal-mustache.jpg";

		using var stream = File.OpenRead(filePath);
		var multiForm = new MultipartFormDataContent
		{
			{ "hero", "Jaina" }
		};
		multiForm.AddFile("file", stream);

		var response = await httpClient.CreateRequest("/api/sample/upload")
			.AsPost()
			.WithBodyContent(multiForm)
			.ReturnAsResponse<UploadResult>();

		response.StatusCode.ShouldBe(HttpStatusCode.OK);
		response.Data!.FileName.ShouldBe("animal-mustache.jpg");
		response.Data!.ContentType.ShouldBe("image/jpeg");
		response.Data!.Size.ShouldBe(3.96875);
	}

	[Fact]
	public async Task MultipartForm_AddFile_Bytes()
	{
		var httpClient = BuildClient();
		var filePath = "./animal-mustache.jpg";
		var fileBytes = File.ReadAllBytes(filePath);
		var fileName = Path.GetFileName(filePath);

		var multiForm = new MultipartFormDataContent
		{
			{ "hero", "Jaina" }
		};
		multiForm.AddFile("file", fileBytes, fileName);

		var response = await httpClient.CreateRequest("/api/sample/upload")
			.AsPost()
			.WithBodyContent(multiForm)
			.ReturnAsResponse<UploadResult>();

		response.StatusCode.ShouldBe(HttpStatusCode.OK);
		response.Data!.FileName.ShouldBe("animal-mustache.jpg");
		response.Data!.ContentType.ShouldBe("image/jpeg");
		response.Data!.Size.ShouldBe(3.96875);
	}
}