namespace FluentlyHttpClient;

internal static class HttpMessageExtensions
{
	private const string RequestIdProperty = "request-id";
	private static readonly HttpRequestOptionsKey<string> RequestIdKey = new(RequestIdProperty);

	internal static FluentHttpRequest ToFluentHttpRequest(
		this HttpRequestMessage request,
		FluentHttpClient client
	)
	{
		var builder = client
				.CreateRequest()
				.WithMethod(request.Method)
				.WithUri(request.RequestUri?.ToString() ?? string.Empty)
			;

		if (request.Content is not null)
			builder.WithBodyContent(request.Content);

		foreach (var prop in request.Options)
		{
			if (prop.Value is not null)
				builder.WithItem(prop.Key, prop.Value);
		}

		return new(builder, request);
	}

	internal static string? GetRequestId(this HttpRequestMessage request)
	{
		if (request.Options.TryGetValue(RequestIdKey, out var requestKey))
			return requestKey;

		return null;
	}

	internal static string AddRequestId(this HttpRequestMessage request, string? id = null)
	{
		var requestId = id ?? Guid.NewGuid().ToString();
		request.Options.Set(RequestIdKey, requestId);

		return requestId;
	}

	internal static FluentHttpResponse ToFluentHttpResponse(this HttpResponseMessage response, IDictionary<object, object>? items = null)
		=> new(response, items);
}