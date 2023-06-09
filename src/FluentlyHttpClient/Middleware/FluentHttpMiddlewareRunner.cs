namespace FluentlyHttpClient.Middleware;

/// <summary>
/// Fluent HTTP middleware client context (per client).
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class FluentHttpMiddlewareClientContext
{
	/// <summary>
	/// Debugger display.
	/// </summary>
	protected string DebuggerDisplay => $"Identifier: '{Identifier}'";

	/// <summary>
	/// Formatters to be used for content negotiation for "Accept" and also sending formats. e.g. (JSON, XML)
	/// </summary>
	public MediaTypeFormatterCollection Formatters { get; }

	/// <summary>
	/// Initializes an instance.
	/// </summary>
	/// <param name="identifier"></param>
	/// <param name="formatters"></param>
	public FluentHttpMiddlewareClientContext(string identifier, MediaTypeFormatterCollection formatters)
	{
		Formatters = formatters;
		Identifier = identifier;
	}

	/// <summary>
	/// Gets the HTTP client identifier
	/// </summary>
	public string Identifier { get; }
}

/// <summary>
/// Fluent HTTP middleware execution invoke context (per invoke).
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class FluentHttpMiddlewareContext
{
	/// <summary>
	/// Debugger display.
	/// </summary>
	protected string DebuggerDisplay => $"Request: {{ {Request} }}";

	/// <summary>
	/// Gets the HTTP request.
	/// </summary>
	public FluentHttpRequest Request { get; set; } = null!;

	internal Func<Task<FluentHttpResponse>> Func { get; set; } = null!;
}

/// <summary>
/// Fluent HTTP middleware invoke delegate.
/// </summary>
/// <param name="context">Middleware context.</param>
/// <returns>Returns async response.</returns>
public delegate Task<FluentHttpResponse> FluentHttpMiddlewareDelegate(FluentHttpMiddlewareContext context);

/// <summary>
/// Fluent HTTP middleware runner/executor interface.
/// </summary>
public interface IFluentHttpMiddlewareRunner
{
	/// <summary>
	/// Run the middleware and lastly the specified action.
	/// </summary>
	/// <param name="request">HTTP request to send.</param>
	/// <param name="action">Action to invoke after middleware are all executed.</param>
	Task<FluentHttpResponse> Run(FluentHttpRequest request, Func<Task<FluentHttpResponse>> action);
}

/// <summary>
/// Fluent HTTP middleware runner/executor class.
/// </summary>
public class FluentHttpMiddlewareRunner
	: IFluentHttpMiddlewareRunner
{
	private readonly IFluentHttpMiddleware _middleware;

	/// <summary>
	/// Initializes a new instance.
	/// </summary>
	/// <param name="middleware">Middleware pipeline to execute.</param>
	public FluentHttpMiddlewareRunner(IFluentHttpMiddleware middleware)
	{
		_middleware = middleware;
	}

	/// <inheritdoc />
	public async Task<FluentHttpResponse> Run(FluentHttpRequest request, Func<Task<FluentHttpResponse>> action)
		=> await _middleware.Invoke(new()
		{
			Func = action,
			Request = request
		}).ConfigureAwait(false);
}