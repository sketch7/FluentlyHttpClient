namespace FluentlyHttpClient;

/// <summary>
/// Fluent HTTP request, which wraps the <see cref="HttpRequestMessage"/> and add additional features.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class FluentHttpRequest : IFluentHttpMessageState
{
	private string DebuggerDisplay => $"[{Method}] '{Uri}'";

	/// <summary>
	/// Gets the underlying HTTP request message.
	/// </summary>
	public HttpRequestMessage Message { get; }

	/// <summary>
	/// Gets the request builder which is responsible for this request message.
	/// </summary>
	public FluentHttpRequestBuilder Builder { get; }

	/// <summary>
	/// Gets or sets the <see cref="HttpMethod"/> for the HTTP request.
	/// </summary>
	public HttpMethod Method
	{
		get => Message.Method;
		set => Message.Method = value;
	}

	/// <summary>
	/// Gets or sets the <see cref="System.Uri"/> for the HTTP request.
	/// </summary>
	public Uri? Uri
	{
		get => Message.RequestUri;
		set => Message.RequestUri = value;
	}

	/// <summary>
	/// Gets the collection of HTTP request headers.
	/// </summary>
	public HttpRequestHeaders Headers => Message.Headers;

	/// <summary>
	/// Determine whether it has success status otherwise it will throw or not.
	/// </summary>
	public bool HasSuccessStatusOrThrow { get; set; }

	/// <summary>
	/// Cancellation token to cancel operation.
	/// </summary>
	public CancellationToken CancellationToken { get; set; }

	/// <inheritdoc />
	public IDictionary<object, object> Items { get; protected set; }

	/// <summary>
	/// Formatters to be used for content negotiation for "Accept" and also sending formats. e.g. (JSON, XML)
	/// </summary>
	[Obsolete("This was added to be passed down to the middleware. Instead in middleware use FluentHttpMiddlewareClientContext.Formatters.")]
	public MediaTypeFormatterCollection? Formatters { get; set; } // deprecated: remove

	/// <summary>
	/// Initializes a new instance.
	/// </summary>
	public FluentHttpRequest(FluentHttpRequestBuilder builder, HttpRequestMessage message, IDictionary<object, object>? items = null) // todo: is items really needed?
	{
		Message = message;
		Builder = builder;
		Items = new Dictionary<object, object>(
			items ?? builder.Items ?? new Dictionary<object, object>()
		);
	}

	/// <summary>
	/// Gets readable request info as string.
	/// </summary>
	public override string ToString() => $"{DebuggerDisplay}";
}