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
	}
}
