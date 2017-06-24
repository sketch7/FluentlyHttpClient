using Xunit;

namespace FluentlyHttpClient.Test
{
	public class FluentHttpRequestBuilderTest
	{
		[Fact]
		public void WithUri_ShouldInterpolate()
		{
			var builder = new FluentHttpRequestBuilder(null);
			builder.WithUri("/org/{org}", new
			{
				org = "sketch7"
			});

			Assert.Equal("/org/sketch7", builder.Uri);
		}

		[Fact]
		public void WithQueryParams_AddQuery()
		{
			var builder = new FluentHttpRequestBuilder(null);
			var request = builder.WithUri("/org/sketch7")
				.AsGet()
				.WithQueryParams(new
				{
					page = 1,
					filter = "all"
				}).Build();

			Assert.Equal("/org/sketch7?page=1&filter=all", request.Url.ToString());
		}

		[Fact]
		public void WithQueryParams_AppendQuery()
		{
			var builder = new FluentHttpRequestBuilder(null);
			var request = builder.WithUri("/org/sketch7?hero=rex")
				.AsGet()
				.WithQueryParams(new
				{
					page = 1,
					filter = "all"
				}).Build();

			Assert.Equal("/org/sketch7?hero=rex&page=1&filter=all", request.Url.ToString());
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