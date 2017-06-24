using Xunit;

namespace FluentlyHttpClient.Test
{
	public class FluentHttpRequestBuilderTest
	{
		[Fact]
		public void WithUri_ShouldInterpolate()
		{
			var request = new FluentHttpRequestBuilder(null)
				.AsGet()
				.WithUri("/org/{org}", new
				{
					org = "sketch7"
				}).Build();

			Assert.Equal("/org/sketch7", request.Url.ToString());
		}

		[Fact]
		public void WithQueryParams_AddQuery()
		{
			var builder = new FluentHttpRequestBuilder(null);
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
		public void WithQueryParams_WithoutLowerKeys()
		{
			var builder = new FluentHttpRequestBuilder(null);
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
		public void WithQueryParams_AppendQuery()
		{
			var builder = new FluentHttpRequestBuilder(null);
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
		public void WithQueryParams_EmptyObject_RemainAsIs()
		{
			var builder = new FluentHttpRequestBuilder(null);
			var request = builder.WithUri("/org/sketch7")
				.AsGet()
				.WithQueryParams(new { })
				.Build();

			Assert.Equal("/org/sketch7", request.Url.ToString());
		}

		[Fact]
		public void BuildValidation_ThrowsErrorWhenMethodNotSpecified()
		{
			var builder = new FluentHttpRequestBuilder(null);
			Assert.Throws<RequestValidationException>(() => builder.WithUri("/org").Build());
		}

		[Fact]
		public void BuildValidation_ThrowsErrorWhenUriNotSpecified()
		{
			var builder = new FluentHttpRequestBuilder(null);
			Assert.Throws<RequestValidationException>(() => builder.AsGet().Build());
		}
	}
}