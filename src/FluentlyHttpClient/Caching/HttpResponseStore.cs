namespace FluentlyHttpClient.Caching;

/// <summary>
/// Contract for storing a serialized HTTP response.
/// </summary>
public interface IHttpResponseStore
{
	/// <summary>Gets or sets the client identifier name.</summary>
	string? Name { get; set; }
	/// <summary>Gets or sets the request hash key.</summary>
	string? Hash { get; set; }
	/// <summary>Gets or sets the request URL.</summary>
	string? Url { get; set; }
	/// <summary>Gets or sets the response body content as a string.</summary>
	string? Content { get; set; }
	/// <summary>Gets or sets the response headers.</summary>
	FluentHttpHeaders? Headers { get; set; }
	/// <summary>Gets or sets the HTTP status code.</summary>
	int StatusCode { get; set; }
	/// <summary>Gets or sets the reason phrase.</summary>
	string? ReasonPhrase { get; set; }
	/// <summary>Gets or sets the HTTP version string.</summary>
	string? Version { get; set; }
	/// <summary>Gets or sets the content headers.</summary>
	FluentHttpHeaders? ContentHeaders { get; set; }
	/// <summary>Gets or sets the original request message.</summary>
	HttpRequestMessage? RequestMessage { get; set; }
}

/// <summary>
/// Default in-memory store for a serialized HTTP response.
/// </summary>
public class HttpResponseStore : IHttpResponseStore
{
	/// <inheritdoc />
	public string? Name { get; set; }
	/// <inheritdoc />
	public string? Hash { get; set; }
	/// <inheritdoc />
	public string? Url { get; set; }
	/// <inheritdoc />
	public string? Content { get; set; }
	/// <inheritdoc />
	public FluentHttpHeaders? Headers { get; set; }
	/// <inheritdoc />
	public int StatusCode { get; set; }
	/// <inheritdoc />
	public string? ReasonPhrase { get; set; }
	/// <inheritdoc />
	public string? Version { get; set; }
	/// <inheritdoc />
	public FluentHttpHeaders? ContentHeaders { get; set; }
	/// <inheritdoc />
	public HttpRequestMessage? RequestMessage { get; set; }
}