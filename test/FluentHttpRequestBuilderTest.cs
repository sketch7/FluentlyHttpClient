using FluentlyHttpClient;
using System.Collections.Generic;
using System.Linq;
using Xunit;
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

			Assert.Equal("en/heroes/azmodan", request.Url.ToString());
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

			Assert.Equal("/org/sketch7?page=1&filter=all", request.Url.ToString());
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

			Assert.Equal("/org/sketch7?Page=1&Filter=all", request.Url.ToString());
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

			Assert.Equal("/org/sketch7?hero=rex&page=1&filter=all", request.Url.ToString());
		}

		[Fact]
		public void EmptyObject_RemainAsIs()
		{
			var builder = NewBuilder();
			var request = builder.WithUri("/org/sketch7")
				.WithQueryParams(new { })
				.Build();

			Assert.Equal("/org/sketch7", request.Url.ToString());
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
}