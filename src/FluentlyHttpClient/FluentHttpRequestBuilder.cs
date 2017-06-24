using System;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FluentlyHttpClient
{
	public class FluentHttpRequestBuilder
	{
		/// <summary>
		/// Gets the Http Method for the Http Request.
		/// </summary>
		public HttpMethod HttpMethod { get; private set; }

		/// <summary>
		/// Gets the Uri used for the HTTP request.
		/// </summary>
		public string Uri { get; private set; }

		/// <summary>
		/// Gets the Uri template for the HTTP request (without interpolation).
		/// </summary>
		public string UriTemplate { get; private set; }

		private readonly FluentHttpClient _fluentHttpClient;
		private HttpContent _httpBody;
		private static readonly HttpMethod HttpMethodPatch = new HttpMethod("Patch");
		private static readonly Regex InterpolationRegex = new Regex(@"\{(\w+)\}", RegexOptions.Compiled);
		private object _queryParams;
		private bool _lowerCaseQueryKeys;

		public FluentHttpRequestBuilder(FluentHttpClient fluentHttpClient)
		{
			_fluentHttpClient = fluentHttpClient;
		}

		#region HttpMethods
		/// <summary>
		/// Set request method as <c>Get</c>.
		/// </summary>
		/// <returns>Returns request builder for chaining.</returns>
		public FluentHttpRequestBuilder AsGet()
		{
			HttpMethod = HttpMethod.Get;
			return this;
		}

		/// <summary>
		/// Set request method as <c>Post</c>.
		/// </summary>
		/// <returns>Returns request builder for chaining.</returns>
		public FluentHttpRequestBuilder AsPost()
		{
			HttpMethod = HttpMethod.Post;
			return this;
		}

		/// <summary>
		/// Set request method as <c>Put</c>.
		/// </summary>
		/// <returns>Returns request builder for chaining.</returns>
		public FluentHttpRequestBuilder AsPut()
		{
			HttpMethod = HttpMethod.Put;
			return this;
		}

		/// <summary>
		/// Set request method as <c>Delete</c>.
		/// </summary>
		/// <returns>Returns request builder for chaining.</returns>
		public FluentHttpRequestBuilder AsDelete()
		{
			HttpMethod = HttpMethod.Delete;
			return this;
		}

		/// <summary>
		/// Set request method as <c>Options</c>.
		/// </summary>
		/// <returns>Returns request builder for chaining.</returns>
		public FluentHttpRequestBuilder AsOptions()
		{
			HttpMethod = HttpMethod.Options;
			return this;
		}

		/// <summary>
		/// Set request method as <c>Head</c>.
		/// </summary>
		/// <returns>Returns request builder for chaining.</returns>
		public FluentHttpRequestBuilder AsHead()
		{
			HttpMethod = HttpMethod.Head;
			return this;
		}

		/// <summary>
		/// Set request method as <c>Trace</c>.
		/// </summary>
		/// <returns>Returns request builder for chaining.</returns>
		public FluentHttpRequestBuilder AsTrace()
		{
			HttpMethod = HttpMethod.Trace;
			return this;
		}

		/// <summary>
		/// Set request method as <c>Patch</c>.
		/// </summary>
		/// <returns>Returns request builder for chaining.</returns>
		public FluentHttpRequestBuilder AsPatch()
		{
			HttpMethod = HttpMethodPatch;
			return this;
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
		#endregion

		/// <summary>
		/// Set the uri of the HTTP request with optional interpolations.
		/// </summary>
		/// <param name="uriTemplate">Uri resource template e.g. <c>"/org/{id}"</c></param>
		/// <param name="interpolationData">Data to interpolate within the Uri template place holders e.g. <c>{id}</c>. Can be either dictionary or object.</param>
		/// <returns></returns>
		public FluentHttpRequestBuilder WithUri(string uriTemplate, object interpolationData = null)
		{
			UriTemplate = uriTemplate;
			Uri = interpolationData != null
				? InterpolationRegex.ReplaceTokens(uriTemplate, interpolationData.ToDictionary())
				: uriTemplate;

			return this;
		}

		/// <summary>
		/// Set query string params to the Uri. e.g. .?page=1&filter=all'.
		/// </summary>
		/// <param name="queryParams">Query data to add/append. Can be either dictionary or object.</param>
		/// <param name="lowerCaseQueryKeys">Determine whether to lowercase query string keys.</param>
		/// <returns></returns>
		public FluentHttpRequestBuilder WithQueryParams(object queryParams, bool lowerCaseQueryKeys = true)
		{
			_lowerCaseQueryKeys = lowerCaseQueryKeys;
			_queryParams = queryParams;
			return this;
		}

		/// <summary>Set the body content of the HTTP request.</summary>
		/// <param name="body">Value to serialize into the HTTP body content.</param>
		/// <param name="contentType">Request body format (or <c>null</c> to use the first supported Content-Type in the <see cref="FluentHttpClient.Formatters"/>).</param>
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
		/// <param name="contentType">Request body format (or <c>null</c> to use the first supported Content-Type in the <see cref="FluentHttpClient.Formatters"/>).</param>
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

		public async Task<T> Return<T>()
		{
			var response = await ReturnAsResponse<T>();
			return response.Data;
		}

		public async Task<FluentHttpResponse<T>> ReturnAsResponse<T>()
		{
			var response = await _fluentHttpClient.Send<T>(this);
			response.Data = await response.RawResponse.Content.ReadAsAsync<T>(_fluentHttpClient.Formatters);
			return response;
		}

		public FluentHttpRequest Build()
		{
			ValidateRequest();

			var uri = BuildUri(Uri, _queryParams, _lowerCaseQueryKeys);
			var httpRequest = new HttpRequestMessage(HttpMethod, uri);
			if (_httpBody != null)
				httpRequest.Content = _httpBody;

			var fluentRequest = new FluentHttpRequest(httpRequest);
			return fluentRequest;
		}

		protected void ValidateRequest()
		{
			if (HttpMethod == null)
				throw RequestValidationException.FieldNotSpecified(nameof(HttpMethod));

			if (string.IsNullOrWhiteSpace(Uri))
				throw RequestValidationException.FieldNotSpecified(nameof(Uri));
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
				queryCollection[lowerCaseQueryKeys ? item.Key.ToLower() : item.Key] = item.Value.ToString();
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