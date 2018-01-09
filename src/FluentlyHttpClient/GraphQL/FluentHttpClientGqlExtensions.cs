using System.Threading.Tasks;
using FluentlyHttpClient.GraphQL;

// ReSharper disable once CheckNamespace
namespace FluentlyHttpClient
{
	/// <summary>
	/// Fluent HTTP request builder GraphQL extensions.
	/// </summary>
	public static class FluentHttpClientGqlExtensions
	{
		/// <summary>
		/// Set request as a GraphQL.
		/// </summary>
		/// <returns></returns>
		public static FluentHttpRequestBuilder AsGql(this FluentHttpRequestBuilder builder, string query) => builder.AsPost().WithBody(new { query });

		/// <summary>
		/// Set request as GraphQL
		/// </summary>
		///<returns>Returns request builder for chaining.</returns>
		public static FluentHttpRequestBuilder AsGql(this FluentHttpRequestBuilder builder, GqlQuery query) => builder.AsPost().WithBody(query);

		/// <summary>
		/// Creates a request for GraphQL.
		/// </summary>
		/// <returns>Returns request builder for chaining.</returns>
		public static FluentHttpRequestBuilder CreateGqlRequest(this IFluentHttpClient fluentHttpClient, string query)
		{
			return fluentHttpClient.CreateRequest()
				.AsPost()
				.WithBody(new { query });
		}

		/// <summary>
		/// Creates a request for GraphQL.
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
		/// <returns>Returns content within GraphQL data response object</returns>
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