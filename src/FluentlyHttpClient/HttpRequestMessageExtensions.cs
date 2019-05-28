using System.Net.Http;

namespace FluentlyHttpClient
{
	internal static class HttpRequestMessageExtensions
	{
		internal static FluentHttpRequest ToFluentlyHttpRequest(
			this HttpRequestMessage request,
			FluentHttpClient client
		)
			=> new FluentHttpRequest(new FluentHttpRequestBuilder(client), request);
	}
}
