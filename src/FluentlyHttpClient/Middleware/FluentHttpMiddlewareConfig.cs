namespace FluentlyHttpClient.Middleware;

/// <summary>
/// HTTP Middleware configuration.
/// </summary>
/// 
/// <param name="Type">Type for the middleware</param>
/// <param name="Args">Arguments for the middleware</param>
public record FluentHttpMiddlewareConfig(

	Type Type,
	object[]? Args = null
);
