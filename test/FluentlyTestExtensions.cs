namespace FluentlyHttpClient.Test;

public static class FluentlyTestExtensions
{
	public static FluentHttpClientBuilder WithMockMessageHandler(this FluentHttpClientBuilder builder, MockHttpMessageHandler? handler = null)
	{
		handler ??= new();
		return builder.WithMessageHandler(handler);
	}
}