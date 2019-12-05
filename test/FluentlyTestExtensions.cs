using RichardSzalay.MockHttp;

namespace FluentlyHttpClient.Test
{
	public static class FluentlyTestExtensions
	{
		public static FluentHttpClientBuilder WithMockMessageHandler(this FluentHttpClientBuilder builder, MockHttpMessageHandler? handler = null)
		{
			handler ??= new MockHttpMessageHandler();
			return builder.WithMessageHandler(handler);
		}
	}
}