using FluentlyHttpClient.GraphQL;

// ReSharper disable once CheckNamespace
namespace FluentlyHttpClient;

/// <summary>
/// Fluent HTTP request builder GraphQL extensions.
/// </summary>
public static class FluentHttpClientGqlExtensions
{
	/// <summary>
	/// Set request as a GraphQL.
	/// </summary>
	/// <returns></returns>
	public static FluentHttpRequestBuilder AsGql(this FluentHttpRequestBuilder builder, string query, string? operationName = null) => builder.AsPost().WithBody(new { query, operationName });

	/// <summary>
	/// Set request as GraphQL.
	/// </summary>
	///<returns>Returns request builder for chaining.</returns>
	public static FluentHttpRequestBuilder AsGql(this FluentHttpRequestBuilder builder, GqlRequest request) => builder.AsPost().WithBody(request);

	/// <summary>
	/// Creates a request for GraphQL.
	/// </summary>
	/// <returns>Returns request builder for chaining.</returns>
	public static FluentHttpRequestBuilder CreateGqlRequest(this IFluentHttpClient fluentHttpClient, string query, string? operationName = null)
		=> fluentHttpClient.CreateRequest().AsGql(query, operationName);

	/// <summary>
	/// Creates a request for GraphQL.
	/// </summary>
	/// <returns></returns>
	public static FluentHttpRequestBuilder CreateGqlRequest(this IFluentHttpClient fluentHttpClient, GqlRequest request)
		=> fluentHttpClient.CreateRequest().AsGql(request);

	/// <summary>
	/// Sends request and unwrap GraphQL data to be available directly in the `.Data`.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <returns>Returns content within GraphQL data response object</returns>
	public static async Task<FluentHttpResponse<T>> ReturnAsGqlResponse<T>(this FluentHttpRequestBuilder builder)
	{
		var response = await builder.ReturnAsResponse<GqlResponse<T>>().ConfigureAwait(false);
		return new(response)
		{
			Data = response.Data != null ? response.Data.Data : default
		};
	}
}