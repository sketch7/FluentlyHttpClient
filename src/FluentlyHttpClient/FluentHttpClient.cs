using FluentlyHttpClient.Middleware;

namespace FluentlyHttpClient;

/// <summary>
/// Interface for sending HTTP requests with a high level fluent API.
/// </summary>
public interface IFluentHttpClient : IDisposable
{
	/// <summary>
	/// Get the identifier (key) for this instance, which is registered with, within the factory.
	/// </summary>
	string Identifier { get; }

	/// <summary>
	/// Gets the base uri address for each request.
	/// </summary>
	string BaseUrl { get; }

	/// <summary>
	/// Underlying HTTP client. This should be avoided from being used,
	/// however if something is not exposed and its really needed, it can be used from here.
	/// </summary>
	HttpClient RawHttpClient { get; }

	/// <summary>
	/// Formatters to be used for content negotiation for "Accept" and also sending formats. e.g. (JSON, XML)
	/// </summary>
	MediaTypeFormatterCollection Formatters { get; }

	/// <summary>
	/// Gets the headers which should be sent with each request.
	/// </summary>
	HttpRequestHeaders Headers { get; }

	/// <summary>
	/// Gets the default formatter to be used when serializing body content. e.g. JSON, XML, etc...
	/// </summary>
	MediaTypeFormatter? DefaultFormatter { get; }

	/// <summary>Get the formatter for an HTTP content type.</summary>
	/// <param name="contentType">The HTTP content type (or <c>null</c> to automatically select one).</param>
	/// <exception cref="InvalidOperationException">No MediaTypeFormatters are available on the API client for this content type.</exception>
	MediaTypeFormatter GetFormatter(MediaTypeHeaderValue? contentType = null);

	/// <summary>
	/// Create a new request builder which can be configured fluently.
	/// </summary>
	/// <param name="uriTemplate">Uri resource template e.g. <c>"/org/{id}"</c></param>
	/// <param name="interpolationData">Data to interpolate within the Uri template place holders e.g. <c>{id}</c>. Can be either dictionary or object.</param>
	/// <returns>Returns a new request builder.</returns>
	FluentHttpRequestBuilder CreateRequest(string? uriTemplate = null, object? interpolationData = null);

	/// <summary>
	/// Creates a new client and inherit options from the current.
	/// </summary>
	/// <param name="identifier">New identifier name (and apply sub-client id formatter) (defaults: '{parentId}.{id})'</param>
	/// <returns>Returns a new client builder instance.</returns>
	FluentHttpClientBuilder CreateClient(string identifier);

	/// <summary>
	/// Build and send HTTP request.
	/// </summary>
	/// <param name="builder">Request builder to build request from.</param>
	/// <returns>Returns HTTP response.</returns>
	Task<FluentHttpResponse> Send(FluentHttpRequestBuilder builder);

	/// <summary>
	/// Send HTTP request.
	/// </summary>
	/// <param name="request">HTTP fluent request to send.</param>
	/// <returns>Returns HTTP response.</returns>
	Task<FluentHttpResponse> Send(FluentHttpRequest request);

	/// <summary>
	/// Send HTTP request.
	/// <para>NOTE: This is only for interoperability, ideally use the Fluently overloads.</para>
	/// </summary>
	/// <param name="request">HTTP request to send.</param>
	/// <returns>Returns HTTP response.</returns>
	Task<FluentHttpResponse> Send(HttpRequestMessage request);
}

/// <summary>
/// Provides a class for sending HTTP requests with a high level fluent API.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class FluentHttpClient : IFluentHttpClient
{
	private string DebuggerDisplay => $"[{Identifier}] BaseUrl: '{BaseUrl}', MiddlewareCount: {_middlewareBuilder.Count}";

	/// <inheritdoc />
	public string Identifier { get; }

	/// <inheritdoc />
	public string BaseUrl { get; }

	/// <inheritdoc />
	public HttpClient RawHttpClient { get; }

	/// <inheritdoc />
	public MediaTypeFormatterCollection Formatters { get; }

	/// <inheritdoc />
	public MediaTypeFormatter? DefaultFormatter { get; }

	/// <inheritdoc />
	public HttpRequestHeaders Headers { get; }

	private readonly Action<FluentHttpRequestBuilder>? _requestBuilderDefaults;
	private readonly FluentHttpClientOptions _options;
	private readonly IFluentHttpClientFactory _clientFactory;
	private readonly IServiceProvider _serviceProvider;
	private readonly IFluentHttpMiddlewareRunner _middlewareRunner;
	private readonly FluentHttpMiddlewareBuilder _middlewareBuilder;
	private readonly RequestTracker _requestTracker;

	/// <summary>
	/// Initializes an instance of <see cref="FluentHttpClient"/>.
	/// </summary>
	/// <param name="options"></param>
	/// <param name="clientFactory"></param>
	/// <param name="serviceProvider"></param>
	public FluentHttpClient(
		FluentHttpClientOptions options,
		IFluentHttpClientFactory clientFactory,
		IServiceProvider serviceProvider
	)
	{
		_options = options;
		_clientFactory = clientFactory;
		_serviceProvider = serviceProvider;
		_requestBuilderDefaults = options.RequestBuilderDefaults;
		_middlewareBuilder = options.MiddlewareBuilder;
		_requestTracker = new();

		Identifier = options.Identifier;
		BaseUrl = options.BaseUrl;
		Formatters = options.Formatters;
		DefaultFormatter = options.DefaultFormatter;

		_middlewareRunner = options.MiddlewareBuilder.Build(this);
		RawHttpClient = Configure(options);
		Headers = RawHttpClient.DefaultRequestHeaders;
	}

	/// <inheritdoc />
	public MediaTypeFormatter GetFormatter(MediaTypeHeaderValue? contentType = null)
	{
		if (!Formatters.Any())
			throw new InvalidOperationException("No media type formatters available.");

		var formatter = contentType != null
			? Formatters.FirstOrDefault(x => x.SupportedMediaTypes.Any(m => m.MediaType == contentType.MediaType))
			: DefaultFormatter ?? Formatters.FirstOrDefault();
		if (formatter == null)
			throw new InvalidOperationException($"No media type formatters are available for '{contentType}' content-type.");

		return formatter;
	}

	/// <inheritdoc />
	public FluentHttpRequestBuilder CreateRequest(string? uriTemplate = null, object? interpolationData = null)
	{
		var builder = ActivatorUtilities.CreateInstance<FluentHttpRequestBuilder>(_serviceProvider, this);
		_requestBuilderDefaults?.Invoke(builder);
		return uriTemplate != null
			? builder.WithUri(uriTemplate, interpolationData)
			: builder;
	}

	/// <inheritdoc />
	public Task<FluentHttpResponse> Send(FluentHttpRequestBuilder builder) => Send(builder.Build());

	/// <inheritdoc />
	public async Task<FluentHttpResponse> Send(FluentHttpRequest request)
	{
		ArgumentNullException.ThrowIfNull(request);

		var requestId = request.Message.AddRequestId();

		_requestTracker.Push(requestId, request);
		await RawHttpClient.SendAsync(request.Message);
		var executionContext = _requestTracker.Pop(requestId);

		request.Message?.Dispose();

		if (request.HasSuccessStatusOrThrow)
			executionContext.Response.EnsureSuccessStatusCode();

		return executionContext.Response;
	}

	public async Task<FluentHttpResponse> Send(HttpRequestMessage request)
	{
		ArgumentNullException.ThrowIfNull(request);

		var requestId = request.AddRequestId();
		await RawHttpClient.SendAsync(request);
		request.Dispose(); // todo: since consumer is passing request message leave it up to him or not? or make global config ShouldDisposeMessageOnSend or so
		var executionContext = _requestTracker.Pop(requestId);
		return executionContext.Response;
	}

	/// <inheritdoc />
	public FluentHttpClientBuilder CreateClient(string identifier)
	{
		// todo: make configurable
		var id = DefaultSubClientIdentityFormatter(Identifier, identifier);
		return _clientFactory.CreateBuilder(id)
			.FromOptions(_options)
			.WithIdentifier(id);
	}

	private HttpClient Configure(FluentHttpClientOptions options)
	{
		var httpHandler = new FluentMiddlewareHttpHandler(
			_middlewareRunner,
			this,
			_requestTracker,
			options.HttpMessageHandler
		);

		var httpClient = new HttpClient(httpHandler);
		if (!string.IsNullOrEmpty(options.BaseUrl))
			httpClient.BaseAddress = new(options.BaseUrl!);

		httpClient.DefaultRequestHeaders.Add(HeaderTypes.Accept, Formatters.SelectMany(x => x.SupportedMediaTypes).Select(x => x.MediaType));
		httpClient.Timeout = options.Timeout;

		httpClient.DefaultRequestHeaders.AddRange(options.Headers);

		return httpClient;
	}

	/// <inheritdoc />
	public void Dispose()
		=> RawHttpClient?.Dispose();

	public static implicit operator HttpClient(FluentHttpClient client)
		=> client.RawHttpClient;

	private static string DefaultSubClientIdentityFormatter(string parentId, string id)
		=> $"{parentId}.{id}";
}