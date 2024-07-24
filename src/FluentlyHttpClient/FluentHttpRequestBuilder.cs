using Microsoft.Extensions.Primitives;
using System.Net;
using System.Text.RegularExpressions;

namespace FluentlyHttpClient;

/// <summary>
/// Class to build <see cref="FluentHttpRequest"/> with a fluent API.
/// </summary>
public class FluentHttpRequestBuilder : IFluentHttpHeaderBuilder<FluentHttpRequestBuilder>, IFluentHttpMessageItems
{
	internal static Version _defaultVersion = HttpVersion.Version20;

	/// <summary>
	/// Gets the HTTP Version for the Http Request.
	/// </summary>
	public Version Version { get; private set; } = _defaultVersion;

	/// <summary>
	/// Gets the HTTP Version for the Http Request.
	/// </summary>
	public HttpVersionPolicy? VersionPolicy { get; private set; }

	/// <summary>
	/// Gets the HTTP Method for the Http Request.
	/// </summary>
	public HttpMethod HttpMethod { get; private set; } = HttpMethod.Get;

	/// <summary>
	/// Gets the Uri used for the HTTP request.
	/// </summary>
	public string? Uri { get; private set; }

	/// <summary>
	/// Gets the Uri template for the HTTP request (without interpolation).
	/// </summary>
	public string? UriTemplate { get; private set; }

	/// <summary>
	/// Gets the headers to be sent with this request.
	/// </summary>
	public FluentHttpHeaders Headers
	{
		get
		{
			_headers ??= [];
			return _headers;
		}
	}

	/// <summary>
	/// Gets the default headers from the HTTP client.
	/// </summary>
	public HttpRequestHeaders DefaultHeaders => _fluentHttpClient.Headers;

	/// <summary>
	/// Gets the base url from the HTTP client.
	/// </summary>
	public string BaseUrl => _fluentHttpClient.BaseUrl;

	/// <inheritdoc />
	/// <summary>
	/// Gets the key/value collection that can be used to share data within the scope of request/response or middleware.
	/// </summary>
	public IDictionary<object, object> Items { get; } = new Dictionary<object, object>();

	private readonly IFluentHttpClient _fluentHttpClient;
	private HttpContent? _httpBody;
	private static readonly Regex InterpolationRegex = new(@"\{(\w+)\}", RegexOptions.Compiled);
	private object? _queryParams;
	private bool _hasSuccessStatusOrThrow;
	private CancellationToken _cancellationToken;
	private QueryStringOptions? _queryStringOptions;
	private FluentHttpHeaders? _headers;

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
	/// Set request HTTP version with specified version.
	/// </summary>
	/// <returns>Returns request builder for chaining.</returns>
	public FluentHttpRequestBuilder WithVersion(Version version)
	{
		Version = version;
		return this;
	}

	/// <summary>
	/// Set request HTTP version policy with specified policy.
	/// </summary>
	/// <returns>Returns request builder for chaining.</returns>
	public FluentHttpRequestBuilder WithVersionPolicy(HttpVersionPolicy policy)
	{
		VersionPolicy = policy;
		return this;
	}

	public FluentHttpRequestBuilder WithHeader(string key, string value)
	{
		Headers.Set(key, value);
		return this;
	}

	public FluentHttpRequestBuilder WithHeader(string key, StringValues values)
	{
		Headers.Set(key, values);
		return this;
	}

	public FluentHttpRequestBuilder WithHeaders(IDictionary<string, string> headers)
	{
		Headers.SetRange(headers);
		return this;
	}

	public FluentHttpRequestBuilder WithHeaders(IDictionary<string, string[]> headers)
	{
		Headers.SetRange(headers);
		return this;
	}

	public FluentHttpRequestBuilder WithHeaders(IDictionary<string, StringValues> headers)
	{
		Headers.SetRange(headers);
		return this;
	}

	public FluentHttpRequestBuilder WithHeaders(FluentHttpHeaders headers)
	{
		Headers.SetRange(headers);
		return this;
	}

	/// <summary>
	/// Set the uri of the HTTP request with optional interpolations.
	/// </summary>
	/// <param name="uriTemplate">Uri resource template e.g. <c>"/org/{id}"</c></param>
	/// <param name="interpolationData">Data to interpolate within the Uri template place holders e.g. <c>{id}</c>. Can be either dictionary or object.</param>
	/// <returns>Returns request builder for chaining.</returns>
	public FluentHttpRequestBuilder WithUri(string uriTemplate, object? interpolationData = null)
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
	/// <param name="options">Query string options to use.</param>
	/// <returns>Returns request builder for chaining.</returns>
	public FluentHttpRequestBuilder WithQueryParams(object queryParams, QueryStringOptions? options = null)
	{
		options ??= _queryStringOptions;

		_queryParams = queryParams;
		return WithQueryParamsOptions(options);
	}

	/// <summary>
	/// Set query string params to the Uri. e.g. .?page=1&amp;filter=all'.
	/// </summary>
	/// <param name="queryParams">Query data to add/append. Can be either dictionary or object.</param>
	/// <param name="configure">Function to configure query string options.</param>
	/// <returns>Returns request builder for chaining.</returns>
	public FluentHttpRequestBuilder WithQueryParams(object queryParams, Action<QueryStringOptions> configure)
	{
		if (configure == null) throw new ArgumentNullException(nameof(configure));
		var options = _queryStringOptions?.Clone() ?? new QueryStringOptions();
		configure(options);
		return WithQueryParams(queryParams, options);
	}

	/// <summary>
	/// Set query string params options.
	/// </summary>
	/// <param name="options">Query string options to use.</param>
	/// <returns>Returns request builder for chaining.</returns>
	public FluentHttpRequestBuilder WithQueryParamsOptions(QueryStringOptions? options)
	{
		_queryStringOptions = options;
		return this;
	}

	/// <summary>
	/// Set query string params options.
	/// </summary>
	/// <param name="configure">Function to configure query string options.</param>
	/// <returns>Returns request builder for chaining.</returns>
	public FluentHttpRequestBuilder WithQueryParamsOptions(Action<QueryStringOptions> configure)
	{
		if (configure == null) throw new ArgumentNullException(nameof(configure));
		var options = _queryStringOptions ?? new QueryStringOptions();
		configure(options);
		return WithQueryParamsOptions(options);
	}

	/// <summary>Set the body content of the HTTP request.</summary>
	/// <param name="body">Value to serialize into the HTTP body content.</param>
	/// <param name="contentType">Request body format (or <c>null</c> to use the first supported Content-Type in the <see cref="IFluentHttpClient.Formatters"/>).</param>
	/// <returns>Returns the request builder for chaining.</returns>
	/// <exception cref="InvalidOperationException">No MediaTypeFormatters are available on the API client for this content type.</exception>
	public FluentHttpRequestBuilder WithBody<T>(T body, MediaTypeHeaderValue? contentType = null)
	{
		var formatter = _fluentHttpClient.GetFormatter(contentType);
		var mediaType = contentType?.MediaType;
		return WithBody(body, formatter, mediaType);
	}

	/// <summary>Set the body content of the HTTP request.</summary>
	/// <param name="body">Value to serialize into the HTTP body content.</param>
	/// <param name="contentType">Request body format (or <c>null</c> to use the first supported Content-Type in the <see cref="IFluentHttpClient.Formatters"/>).</param>
	/// <exception cref="InvalidOperationException">No MediaTypeFormatters are available on the API client for this content type.</exception>
	/// <returns>Returns the request builder for chaining.</returns>
	public FluentHttpRequestBuilder WithBody(object body, MediaTypeHeaderValue? contentType = null)
	{
		var formatter = _fluentHttpClient.GetFormatter(contentType);
		var mediaType = contentType?.MediaType;
		return WithBody(body, formatter, mediaType);
	}

	/// <summary>Set the body content of the HTTP request.</summary>
	/// <param name="body">Value to serialize into the HTTP body content.</param>
	/// <param name="formatter">Media type formatter with which to format the request body format.</param>
	/// <param name="mediaType">HTTP media type (or <c>null</c> for the <paramref name="formatter"/>'s default).</param>
	/// <returns>Returns the request builder for chaining.</returns>
	public FluentHttpRequestBuilder WithBody(object body, MediaTypeFormatter formatter, string? mediaType = null)
		=> WithBodyContent(new ObjectContent(body.GetType(), body, formatter, mediaType));

	/// <summary>Set the body content of the HTTP request.</summary>
	/// <param name="body">Value to serialize into the HTTP body content.</param>
	/// <param name="formatter">Media type formatter with which to format the request body format.</param>
	/// <param name="mediaType">HTTP media type (or <c>null</c> for the <paramref name="formatter"/>'s default).</param>
	/// <returns>Returns the request builder for chaining.</returns>
	public FluentHttpRequestBuilder WithBody<T>(T body, MediaTypeFormatter formatter, string? mediaType = null)
		=> WithBodyContent(new ObjectContent<T>(body, formatter, mediaType));

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
	public async Task<T?> Return<T>()
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
		{
			genericResponse.Data = await genericResponse.Content.ReadAsAsync<T>(_fluentHttpClient.Formatters, _cancellationToken)
				.ConfigureAwait(false);
		}

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

		Uri ??= string.Empty;
		var uri = BuildUri(Uri, _queryParams, _queryStringOptions);
		var httpRequest = new HttpRequestMessage(HttpMethod, uri)
		{
			Version = Version,
		};
		if (VersionPolicy.HasValue)
			httpRequest.VersionPolicy = VersionPolicy.Value;

		if (_httpBody != null)
			httpRequest.Content = _httpBody;

		if (_headers != null)
		{
			foreach (var header in _headers)
			{
				if (header.Key == HeaderTypes.UserAgent)
					httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
				else
					httpRequest.Headers.Add(header.Key, header.Value);
			}
		}

		return new(this, httpRequest, Items)
		{
			HasSuccessStatusOrThrow = _hasSuccessStatusOrThrow,
			CancellationToken = _cancellationToken,
			Formatters = _fluentHttpClient.Formatters
		};
	}

	/// <summary>
	/// Ensure validate request or throw.
	/// </summary>
	/// <exception cref="RequestValidationException">When request is not valid.</exception>
	protected void ValidateRequest()
	{
		if (HttpMethod == null)
			throw RequestValidationException.FieldNotSpecified(nameof(HttpMethod));

		if (HttpMethod == HttpMethod.Get && _httpBody != null)
			throw new RequestValidationException("A request with Method 'GET' cannot have a body assigned.");
	}

	private static string BuildQueryString(object? queryParams, QueryStringOptions? options)
	{
		if (queryParams == null)
			return string.Empty;

		var dict = queryParams.ToDictionary();
		if (dict.Count == 0)
			return string.Empty;

		var queryCollection = new Dictionary<string, object>();
		foreach (var item in dict)
			queryCollection[item.Key] = item.Value;

		return queryCollection.ToQueryString(options);
	}

	private static string BuildUri(string uri, object? queryParams, QueryStringOptions? options)
	{
		var queryString = BuildQueryString(queryParams, options);
		if (string.IsNullOrEmpty(queryString))
			return uri;

		if (uri.Contains('?'))
			uri += $"&{queryString}";
		else
			uri += $"?{queryString}";
		return uri;
	}
}