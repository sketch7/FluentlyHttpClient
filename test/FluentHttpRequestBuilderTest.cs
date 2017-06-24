using FluentlyHttpClient;
using Xunit;
using static Test.FluentHttpRequestBuilderTestUtil;

namespace Test
{
	public static class FluentHttpRequestBuilderTestUtil
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
				.AsGet()
				.WithUri("/org/{org}", new
				{
					org = "sketch7"
				}).Build();

			Assert.Equal("/org/sketch7", request.Url.ToString());
		}
	}

	public class RequestBuilder_WithQueryParams
	{
		[Fact]
		public void AddQuery()
		{
			var builder = NewBuilder();
			var request = builder.WithUri("/org/sketch7")
				.AsGet()
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
				.AsGet()
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
				.AsGet()
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
				.AsGet()
				.WithQueryParams(new
				{
				})
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
			Assert.Throws<RequestValidationException>(() => builder.WithUri("/org").Build());
		}

		[Fact]
		public void ThrowsErrorWhenUriNotSpecified()
		{
			var builder = NewBuilder();
			Assert.Throws<RequestValidationException>(() => builder.AsGet().Build());
		}
	}
}