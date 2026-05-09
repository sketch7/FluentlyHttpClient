using FluentlyHttpClient.Caching;

namespace FluentlyHttpClient.Entity;

/// <summary>EF Core entity representing a cached HTTP response.</summary>
public class HttpResponseEntity : IHttpResponseStore
{
	/// <summary>Gets or sets the primary key (hashed request identifier).</summary>
	public string? Id { get; set; }
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