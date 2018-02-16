using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace FluentlyHttpClient
{
	/// <summary>
	/// Class to build <see cref="FluentHttpRequest"/> with a fluent API.
	/// </summary>
	public class FluentHttpRequestBuilder : IFluentHttpHeaderBuilder<FluentHttpRequestBuilder>
	{
		/// <summary>
		/// Gets the HTTP Method for the Http Request.
		/// </summary>
		public HttpMethod HttpMethod { get; private set; } = HttpMethod.Get;

		/// <summary>
		/// Gets the Uri used for the HTTP request.
		/// </summary>
		public string Uri { get; private set; }

		/// <summary>
		/// Gets the Uri template for the HTTP request (without interpolation).
		/// </summary>
		public string UriTemplate { get; private set; }

		/// <summary>
		/// Get the headers to be sent with this request.
		/// </summary>
		public Dictionary<string, string> Headers { get; private set; }

		/// <summary>
		/// Get the key/value collection that can be used to share data within the scope of request/response or middleware.
		/// </summary>
		public Dictionary<object, object> Items { get; } = new Dictionary<object, object>();

		private readonly IFluentHttpClient _fluentHttpClient;
		private HttpContent _httpBody;
		private static readonly Regex InterpolationRegex = new Regex(@"\{(\w+)\}", RegexOptions.Compiled);
		private object _queryParams;
		private bool _lowerCaseQueryKeys;
		private bool _hasSuccessStatusOrThrow;
		private CancellationToken _cancellationToken;

		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		public FluentHttpRequestBuilder(IFluentHttpClient fluentHttpClient)
		{
			_fluentHttpClient = fluentHttpClient;
		}

		/// <summary>
		/// Set request method with specified method.
		/// </summary>
		/// <returns>Returns request builder for chaining.</returns>
		public FluentHttpRequestBuilder WithMethod(HttpMethod method)
		{
			HttpMethod = method;
			return this;
		}

		/// <summary>
		/// Add the specified header and its value for the request.
		/// </summary>
		/// <param name="key">Header to add.</param>
		/// <param name="value">Value for the header.</param>
		/// <returns>Returns request builder for chaining.</returns>
		public FluentHttpRequestBuilder WithHeader(string key, string value)
		{
			if (Headers == null)
				Headers = new Dictionary<string, string>();
			Headers[key] = value;
			return this;
		}

		/// <summary>
		/// Add the specified headers and their value for the request.
		/// </summary>
		/// <param name="headers">Headers to add.</param>
		/// <returns>Returns request builder for chaining.</returns>
		public FluentHttpRequestBuilder WithHeaders(IDictionary<string, string> headers)
		{
			foreach (var item in headers)
				WithHeader(item.Key, item.Value);
			return this;
		}

		/// <summary>
		/// Set the uri of the HTTP request with optional interpolations.
		/// </summary>
		/// <param name="uriTemplate">Uri resource template e.g. <c>"/org/{id}"</c></param>
		/// <param name="interpolationData">Data to interpolate within the Uri template place holders e.g. <c>{id}</c>. Can be either dictionary or object.</param>
		/// <returns>Returns request builder for chaining.</returns>
		public FluentHttpRequestBuilder WithUri(string uriTemplate, object interpolationData = null)
		{
			UriTemplate = uriTemplate;
			Uri = interpolationData != null
				? InterpolationRegex.ReplaceTokens(uriTemplate, interpolationData.ToDictionary())
				: uriTemplate;
			return this;
		}

		/// <summary>
		/// Set query string params to the Uri. e.g. .?page=1&amp;filter=all'.
		/// </summary>
		/// <param name="queryParams">Query data to add/append. Can be either dictionary or object.</param>
		/// <param name="lowerCaseQueryKeys">Determine whether to lowercase query string keys.</param>
		/// <returns>Returns request builder for chaining.</returns>
		public FluentHttpRequestBuilder WithQueryParams(object queryParams, bool lowerCaseQueryKeys = true)
		{
			_lowerCaseQueryKeys = lowerCaseQueryKeys;
			_queryParams = queryParams;
			return this;
		}

		/// <summary>Set the body content of the HTTP request.</summary>
		/// <param name="body">Value to serialize into the HTTP body content.</param>
		/// <param name="contentType">Request body format (or <c>null</c> to use the first supported Content-Type in the <see cref="IFluentHttpClient.Formatters"/>).</param>
		/// <returns>Returns the request builder for chaining.</returns>
		/// <exception cref="InvalidOperationException">No MediaTypeFormatters are available on the API client for this content type.</exception>
		public FluentHttpRequestBuilder WithBody<T>(T body, MediaTypeHeaderValue contentType = null)
		{
			MediaTypeFormatter formatter = _fluentHttpClient.GetFormatter(contentType);
			string mediaType = contentType?.MediaType;
			return WithBody(body, formatter, mediaType);
		}

		/// <summary>Set the body content of the HTTP request.</summary>
		/// <param name="body">Value to serialize into the HTTP body content.</param>
		/// <param name="contentType">Request body format (or <c>null</c> to use the first supported Content-Type in the <see cref="IFluentHttpClient.Formatters"/>).</param>
		/// <exception cref="InvalidOperationException">No MediaTypeFormatters are available on the API client for this content type.</exception>
		/// <returns>Returns the request builder for chaining.</returns>
		public FluentHttpRequestBuilder WithBody(object body, MediaTypeHeaderValue contentType = null)
		{
			MediaTypeFormatter formatter = _fluentHttpClient.GetFormatter(contentType);
			string mediaType = contentType?.MediaType;
			return WithBody(body, formatter, mediaType);
		}

		/// <summary>Set the body content of the HTTP request.</summary>
		/// <param name="body">Value to serialize into the HTTP body content.</param>
		/// <param name="formatter">Media type formatter with which to format the request body format.</param>
		/// <param name="mediaType">HTTP media type (or <c>null</c> for the <paramref name="formatter"/>'s default).</param>
		/// <returns>Returns the request builder for chaining.</returns>
		public FluentHttpRequestBuilder WithBody(object body, MediaTypeFormatter formatter, string mediaType = null)
		{
			return WithBodyContent(new ObjectContent(body.GetType(), body, formatter, mediaType));
		}

		/// <summary>Set the body content of the HTTP request.</summary>
		/// <param name="body">Value to serialize into the HTTP body content.</param>
		/// <param name="formatter">Media type formatter with which to format the request body format.</param>
		/// <param name="mediaType">HTTP media type (or <c>null</c> for the <paramref name="formatter"/>'s default).</param>
		/// <returns>Returns the request builder for chaining.</returns>
		public FluentHttpRequestBuilder WithBody<T>(T body, MediaTypeFormatter formatter, string mediaType = null)
		{
			return WithBodyContent(new ObjectContent<T>(body, formatter, mediaType));
		}

		/// <summary>Set the body content of the HTTP request.</summary>
		/// <param name="body">Formatted HTTP body content.</param>
		/// <returns>Returns the request builder for chaining.</returns>
		public FluentHttpRequestBuilder WithBodyContent(HttpContent body)
		{
			_httpBody = body;
			return this;
		}

		/// <summary>Determine whether the status code should succeeds or else throw.</summary>
		/// <param name="hasSuccessStatusOrThrow">When true status should succeed otherwise it will throw.</param>
		/// <returns>Returns the request builder for chaining.</returns>
		public FluentHttpRequestBuilder WithSuccessStatus(bool hasSuccessStatusOrThrow = true)
		{
			_hasSuccessStatusOrThrow = hasSuccessStatusOrThrow;
			return this;
		}

		/// <summary>Set cancellation token for the request.</summary>
		/// <returns>Returns the request builder for chaining.</returns>
		public FluentHttpRequestBuilder WithCancellationToken(CancellationToken cancellationToken)
		{
			_cancellationToken = cancellationToken;
			return this;
		}

		/// <summary>Set custom item that can be used to share data within the scope of request, response, and middleware.</summary>
		/// <returns>Returns the request builder for chaining.</returns>
		public FluentHttpRequestBuilder WithItem(object key, object value)
		{
			Items[key] = value;
			return this;
		}

		/// <summary>
		/// Send request, read content with the type specified (when success) and return data directly.
		/// </summary>
		/// <typeparam name="T">Type to return.</typeparam>
		/// <returns></returns>
		public async Task<T> Return<T>()
		{
			var response = await ReturnAsResponse<T>().ConfigureAwait(false);
			return response.Data;
		}

		/// <summary>
		/// Send request and returns HTTP Response and also read content with the type specified (when success).
		/// </summary>
		/// <typeparam name="T">Type to return.</typeparam>
		/// <returns>Return response with data typed.</returns>
		public async Task<FluentHttpResponse<T>> ReturnAsResponse<T>()
		{
			var response = await ReturnAsResponse().ConfigureAwait(false);
			var genericResponse = new FluentHttpResponse<T>(response);

			if (genericResponse.IsSuccessStatusCode)
				genericResponse.Data = await genericResponse.Content.ReadAsAsync<T>(_fluentHttpClient.Formatters, _cancellationToken)
											.ConfigureAwait(false);

			return genericResponse;
		}

		/// <summary>
		/// Send request and returns HTTP Response.
		/// </summary>
		/// <returns>Returns an HTTP response.</returns>
		public Task<FluentHttpResponse> ReturnAsResponse() => _fluentHttpClient.Send(this);

		/// <summary>
		/// Build HTTP request.
		/// </summary>
		/// <returns>Return HTTP request instance.</returns>
		public FluentHttpRequest Build()
		{
			ValidateRequest();

			var uri = BuildUri(Uri, _queryParams, _lowerCaseQueryKeys);
			var httpRequest = new HttpRequestMessage(HttpMethod, uri);
			if (_httpBody != null)
				httpRequest.Content = _httpBody;

			if (Headers != null)
				foreach (var header in Headers)
					httpRequest.Headers.Add(header.Key, header.Value);

			var fluentRequest = new FluentHttpRequest(httpRequest, Items)
			{
				HasSuccessStatusOrThrow = _hasSuccessStatusOrThrow,
				CancellationToken = _cancellationToken,
				Formatters = _fluentHttpClient.Formatters
			};
			return fluentRequest;
		}

		/// <summary>
		/// Ensure validate request or throw.
		/// </summary>
		/// <exception cref="RequestValidationException">When request is not valid.</exception>
		protected void ValidateRequest()
		{
			if (HttpMethod == null)
				throw RequestValidationException.FieldNotSpecified(nameof(HttpMethod));

			if (string.IsNullOrWhiteSpace(Uri))
				throw RequestValidationException.FieldNotSpecified(nameof(Uri));

			if (HttpMethod == HttpMethod.Get && _httpBody != null)
				throw new RequestValidationException("A request with Method 'GET' cannot have a body assigned.");
		}

		private static string BuildQueryString(object queryParams, bool lowerCaseQueryKeys)
		{
			if (queryParams == null)
				return string.Empty;

			var dict = queryParams.ToDictionary();
			if (dict.Count == 0)
				return string.Empty;

			var queryCollection = new HttpValueCollection();
			foreach (var item in dict)
			{
				if (item.Value == null) continue;

				queryCollection[lowerCaseQueryKeys ? item.Key.ToLower() : item.Key] = item.Value.ToString();
			}
			return queryCollection.ToString();
		}

		private static string BuildUri(string uri, object queryParams, bool lowerCaseQueryKeys)
		{
			var queryString = BuildQueryString(queryParams, lowerCaseQueryKeys);
			if (string.IsNullOrEmpty(queryString))
				return uri;

			if (uri.Contains("?"))
				uri += $"&{queryString}";
			else
				uri += $"?{queryString}";
			return uri;
		}
	}
}