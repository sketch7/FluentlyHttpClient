namespace FluentlyHttpClient;

/// <summary>
/// HTTP header types such as User-Agent, Authorization, etc...
/// </summary>
public static class HeaderTypes
{
	/// <summary>
	/// Gets the Accept header name.
	/// </summary>
	public const string Accept = "Accept";

	/// <summary>
	/// Gets the Accept-Language header name.
	/// </summary>
	public const string AcceptLanguage = "Accept-Language";

	/// <summary>
	/// Gets the Authorization header name.
	/// </summary>
	public const string Authorization = "Authorization";

	/// <summary>
	/// Gets the Cache-Control header name.
	/// </summary>
	public const string CacheControl = "Cache-Control";

	/// <summary>
	/// Gets the Content-Type header name.
	/// </summary>
	public const string ContentType = "Content-Type";

	/// <summary>
	/// Gets the User-Agent header name.
	/// </summary>
	public const string UserAgent = "User-Agent";

	/// <summary>
	/// Gets the X-Forwarded-For header name.
	/// </summary>
	public const string XForwardedFor = "X-Forwarded-For";

	/// <summary>
	/// Gets the X-Forwarded-Host header name.
	/// </summary>
	public const string XForwardedHost = "X-Forwarded-Host";
}

/// <summary>
/// Auth Schemes types such as Bearer.
/// </summary>
public static class AuthSchemeTypes
{
	/// <summary>
	/// Gets the basic scheme.
	/// </summary>
	public const string Basic = "Basic";

	/// <summary>
	/// Gets the bearer scheme.
	/// </summary>
	public const string Bearer = "Bearer";

	/// <summary>
	/// Gets the digest scheme.
	/// </summary>
	public const string Digest = "Digest";
}