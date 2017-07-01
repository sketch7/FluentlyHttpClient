using FluentlyHttpClient;
using FluentlyHttpClient.Test;
using RichardSzalay.MockHttp;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Xunit;
using static FluentlyHttpClient.Test.ServiceTestUtil;
using static Test.RequestBuilderTestUtil;

namespace Test
{
	public static class RequestBuilderTestUtil
	{
		public static FluentHttpRequestBuilder NewBuilder()
			=> new FluentHttpRequestBuilder(null);
	}

	public class RequestBuilder_WithUri
	{
		[Fact]
		public void ShouldInterpolate()
		{
			var request = NewBuilder()
				.WithUri("{Language}/heroes/{Hero}", new
				{
					Language = "en",
					Hero = "azmodan"
				}).Build();

			Assert.Equal("en/heroes/azmodan", request.Uri.ToString());
		}
	}

	public class RequestBuilder_WithQueryParams
	{
		[Fact]
		public void AddQuery()
		{
			var builder = NewBuilder();
			var request = builder.WithUri("/org/sketch7")
				.WithQueryParams(new
				{
					Page = 1,
					Filter = "all"
				}).Build();

			Assert.Equal("/org/sketch7?page=1&filter=all", request.Uri.ToString());
		}

		[Fact]
		public void AddWithoutLowerKeys()
		{
			var builder = NewBuilder();
			var request = builder.WithUri("/org/sketch7")
				.WithQueryParams(new
				{
					Page = 1,
					Filter = "all"
				}, lowerCaseQueryKeys: false).Build();

			Assert.Equal("/org/sketch7?Page=1&Filter=all", request.Uri.ToString());
		}

		[Fact]
		public void AppendQuery()
		{
			var builder = NewBuilder();
			var request = builder.WithUri("/org/sketch7?hero=rex")
				.WithQueryParams(new
				{
					Page = 1,
					Filter = "all"
				}).Build();

			Assert.Equal("/org/sketch7?hero=rex&page=1&filter=all", request.Uri.ToString());
		}

		[Fact]
		public void EmptyObject_RemainAsIs()
		{
			var builder = NewBuilder();
			var request = builder.WithUri("/org/sketch7")
				.WithQueryParams(new { })
				.Build();

			Assert.Equal("/org/sketch7", request.Uri.ToString());
		}
	}

	public class RequestBuilder_BuildValidation
	{
		[Fact]
		public void ThrowsErrorWhenMethodNotSpecified()
		{
			var builder = NewBuilder();
			Assert.Throws<RequestValidationException>(() => builder.WithMethod(null).WithUri("/org").Build());
		}

		[Fact]
		public void ThrowsErrorWhenUriNotSpecified()
		{
			var builder = NewBuilder();
			Assert.Throws<RequestValidationException>(() => builder.Build());
		}
	}

	public class RequestBuilder_WithHeaders
	{
		[Fact]
		public void AddHeader()
		{
			var builder = NewBuilder()
				.WithUri("/org/sketch7")
				.WithHeader("chiko", "hex")
				;
			var request = builder.Build();

			var header = request.Headers.GetValues("chiko").FirstOrDefault();
			Assert.NotNull(header);
			Assert.Equal("hex", header);
		}

		[Fact]
		public void AddAlreadyExistsHeader_ShouldReplace()
		{
			var builder = NewBuilder()
					.WithUri("/org/sketch7")
					.WithHeader("chiko", "hex")
					.WithHeader("chiko", "hexII")
				;
			var request = builder.Build();

			var header = request.Headers.GetValues("chiko").FirstOrDefault();
			Assert.NotNull(header);
			Assert.Equal("hexII", header);
		}

		[Fact]
		public void AddHeaders()
		{
			var builder = NewBuilder()
					.WithUri("/org/sketch7")
					.WithHeader("chiko", "hex")
					.WithHeaders(new Dictionary<string, string>
					{
						["chiko"] = "hexII",
						["locale"] = "mt-MT"
					})
				;
			var request = builder.Build();

			var chikoHeader = request.Headers.GetValues("chiko").FirstOrDefault();
			var localeHeader = request.Headers.GetValues("locale").FirstOrDefault();
			Assert.NotNull(chikoHeader);
			Assert.Equal("hexII", chikoHeader);
			Assert.NotNull(localeHeader);
			Assert.Equal("mt-MT", localeHeader);
		}
	}

	public class RequestBuilder_Return
	{

		[Fact]
		public async void ReturnAsString()
		{
			var mockResponse = "{ 'name': 'Azmodan' }";
			var mockHttp = new MockHttpMessageHandler();
			mockHttp.When("https://sketch7.com/api/heroes/azmodan")
				.Respond("application/json", mockResponse);

			var fluentHttpClientFactory = GetNewClientFactory();
			fluentHttpClientFactory.CreateBuilder("sketch7")
				.WithBaseUrl("https://sketch7.com")
				.WithMessageHandler(mockHttp)
				.Register();

			var httpClient = fluentHttpClientFactory.Get("sketch7");
			var response = await httpClient.CreateRequest("/api/heroes/azmodan")
				.ReturnAsString();

			Assert.NotNull(response);
			Assert.Equal(mockResponse, response);
		}
	}


	public class RequestBuilder_PostWithBody
	{
		[Fact]
		public async void ReturnAsString()
		{
			var mockResponse = "{ 'name': 'Azmodan' }";
			var mockHttp = new MockHttpMessageHandler();
			mockHttp.When(HttpMethod.Post, "https://sketch7.com/api/heroes/azmodan")
				.WithContent("{\"Title\":\"Lord of Sin\"}")
				.Respond("application/json", "{ 'name': 'Azmodan', 'title': 'Lord of Sin' }");

			var fluentHttpClientFactory = GetNewClientFactory();
			fluentHttpClientFactory.CreateBuilder("sketch7")
				.WithBaseUrl("https://sketch7.com")
				.WithMessageHandler(mockHttp)
				.Register();

			var httpClient = fluentHttpClientFactory.Get("sketch7");
			var response = await httpClient.CreateRequest("/api/heroes/azmodan")
				.AsPost()
				.WithBody(new
				{
					Title = "Lord of Sin"
				})
				.ReturnAsResponse<Hero>();

			response.EnsureSuccessStatusCode();
			Assert.NotNull(response);
			Assert.Equal("Lord of Sin", response.Data.Title);
		}
	}
}