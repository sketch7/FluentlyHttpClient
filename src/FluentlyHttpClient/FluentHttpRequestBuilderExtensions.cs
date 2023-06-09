namespace FluentlyHttpClient;

/// <summary>
/// Fluent HTTP request builder extensions.
/// </summary>
public static class FluentHttpRequestBuilderExtensions
{
	private static readonly HttpMethod HttpMethodPatch = new("Patch");
	#region HttpMethods

	/// <summary>
	/// Set request method as <c>Get</c>.
	/// </summary>
	/// <returns>Returns request builder for chaining.</returns>
	public static FluentHttpRequestBuilder AsGet(this FluentHttpRequestBuilder builder) => builder.WithMethod(HttpMethod.Get);

	/// <summary>
	/// Set request method as <c>Post</c>.
	/// </summary>
	/// <returns>Returns request builder for chaining.</returns>
	public static FluentHttpRequestBuilder AsPost(this FluentHttpRequestBuilder builder) => builder.WithMethod(HttpMethod.Post);

	/// <summary>
	/// Set request method as <c>Put</c>.
	/// </summary>
	/// <returns>Returns request builder for chaining.</returns>
	public static FluentHttpRequestBuilder AsPut(this FluentHttpRequestBuilder builder) => builder.WithMethod(HttpMethod.Put);

	/// <summary>
	/// Set request method as <c>Delete</c>.
	/// </summary>
	/// <returns>Returns request builder for chaining.</returns>
	public static FluentHttpRequestBuilder AsDelete(this FluentHttpRequestBuilder builder) => builder.WithMethod(HttpMethod.Delete);

	/// <summary>
	/// Set request method as <c>Options</c>.
	/// </summary>
	/// <returns>Returns request builder for chaining.</returns>
	public static FluentHttpRequestBuilder AsOptions(this FluentHttpRequestBuilder builder) => builder.WithMethod(HttpMethod.Options);

	/// <summary>
	/// Set request method as <c>Head</c>.
	/// </summary>
	/// <returns>Returns request builder for chaining.</returns>
	public static FluentHttpRequestBuilder AsHead(this FluentHttpRequestBuilder builder) => builder.WithMethod(HttpMethod.Head);

	/// <summary>
	/// Set request method as <c>Trace</c>.
	/// </summary>
	/// <returns>Returns request builder for chaining.</returns>
	public static FluentHttpRequestBuilder AsTrace(this FluentHttpRequestBuilder builder) => builder.WithMethod(HttpMethod.Trace);

	/// <summary>
	/// Set request method as <c>Patch</c>.
	/// </summary>
	/// <returns>Returns request builder for chaining.</returns>
	public static FluentHttpRequestBuilder AsPatch(this FluentHttpRequestBuilder builder) => builder.WithMethod(HttpMethodPatch);

	#endregion

	/// <summary>
	/// Send request and read content as string.
	/// </summary>
	/// <returns>Returns content as string.</returns>
	public static async Task<string> ReturnAsString(this FluentHttpRequestBuilder builder)
	{
		var response = await builder.ReturnAsResponse().ConfigureAwait(false);
		return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
	}

	/// <summary>
	/// Send request and return as stream.
	/// </summary>
	/// <returns>Returns content as stream.</returns>
	public static async Task<Stream> ReturnAsStream(this FluentHttpRequestBuilder builder)
	{
		var response = await builder.ReturnAsResponse().ConfigureAwait(false);
		return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
	}

	/// <summary>
	/// Send request and return as byte array.
	/// </summary>
	/// <returns>Returns content as byte array.</returns>
	public static async Task<byte[]> ReturnAsByteArray(this FluentHttpRequestBuilder builder)
	{
		var response = await builder.ReturnAsResponse().ConfigureAwait(false);
		return await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
	}
}