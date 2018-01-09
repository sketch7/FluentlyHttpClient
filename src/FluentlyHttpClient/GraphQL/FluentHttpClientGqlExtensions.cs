using System.Threading.Tasks;

namespace FluentlyHttpClient
{
	/// <summary>
	/// Fluent HTTP request builder GraphQL extensions.
	/// </summary>
	public static class FluentHttpClientGqlExtensions
	{
		/// <summary>
		/// Set request as a graphql.
		/// </summary>
		/// <returns></returns>
		public static FluentHttpRequestBuilder AsGql(this FluentHttpRequestBuilder builder, string query) => builder.AsPost().WithBody(new { query });

		/// <summary>
		/// Set request as graphql
		/// </summary>
		///<returns>Returns request builder for chaining.</returns>
		public static FluentHttpRequestBuilder AsGql(this FluentHttpRequestBuilder builder, GqlQuery query) => builder.AsPost().WithBody(query);

		/// <summary>
		/// Creates a request for graphql.
		/// </summary>
		/// <returns>Returns request builder for chaining.</returns>
		public static FluentHttpRequestBuilder CreateGqlRequest(this IFluentHttpClient fluentHttpClient, string query)
		{
			return fluentHttpClient.CreateRequest()
				.AsPost()
				.WithBody(new { query });
		}

		/// <summary>
		/// Creates a request for graphql.
		/// </summary>
		/// <returns></returns>
		public static FluentHttpRequestBuilder CreateGqlRequest(this IFluentHttpClient fluentHttpClient, GqlQuery query)
		{
			return fluentHttpClient.CreateRequest()
				.AsPost()
				.WithBody(query);
		}

		/// <summary>
		/// Send request and return as GraphQL response.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns>Returns content within Graphql data response object</returns>
		public static async Task<FluentHttpResponse<T>> ReturnAsGqlResponse<T>(this FluentHttpRequestBuilder builder)
		{
			var response = await builder.ReturnAsResponse<GqlResponse<T>>().ConfigureAwait(false);
			return new FluentHttpResponse<T>(response)
			{
				Data = response.Data.Data
			};
		}
	}
}