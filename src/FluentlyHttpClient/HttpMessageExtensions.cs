using System;
using System.Collections.Generic;
using System.Net.Http;

namespace FluentlyHttpClient
{
	internal static class HttpMessageExtensions
	{
		private const string RequestIdProperty = "request-id";

		internal static FluentHttpRequest ToFluentHttpRequest(
			this HttpRequestMessage request,
			FluentHttpClient client
		)
		{
			var builder = client
				.CreateRequest()
				.WithMethod(request.Method)
				.WithUri(request.RequestUri.ToString())
				.WithBodyContent(request.Content)
			;

			foreach (var prop in request.Properties)
				builder.WithItem(prop.Key, prop.Value);

			return new FluentHttpRequest(builder, request);
		}

		internal static string? GetRequestId(this HttpRequestMessage request)
		{
			if (request.Properties.TryGetValue(RequestIdProperty, out var requestKey))
				return (string)requestKey;

			return null;
		}

		internal static string AddRequestId(this HttpRequestMessage request, string? id = null)
		{
			var requestId = id ?? Guid.NewGuid().ToString();
			request.Properties.Add(RequestIdProperty, requestId);

			return requestId;
		}

		internal static FluentHttpResponse ToFluentHttpResponse(
			this HttpResponseMessage response,
			IDictionary<object, object>? items = null
		) => new FluentHttpResponse(response, items);
	}
}
