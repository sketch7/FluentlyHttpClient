using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace FluentlyHttpClient
{
	/// <summary>
	/// Fluent HTTP request builder extensions.
	/// </summary>
	public static class FluentHttpRequestBuilderExtensions
	{
		private static readonly HttpMethod HttpMethodPatch = new HttpMethod("Patch");
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
		/// Creates a request using graphql.
		/// </summary>
		/// <returns>Returns request builder for chaining.</returns>
		public static FluentHttpRequestBuilder CreateGqlRequest(
			this IFluentHttpClient fluentHttpClient,
			string query,
			string endPoint = "api/graphql"
		)
		{
			return fluentHttpClient.CreateRequest()
				.WithUri(endPoint)
				.AsPost()
				.WithBody(new { query });
		}

		/// <summary>
		/// Creates a request using graphql.
		/// </summary>
		/// <returns></returns>
		public static FluentHttpRequestBuilder CreateGqlRequest(
			this IFluentHttpClient fluentHttpClient,
			GqlQuery query,
			string endPoint = "api/graphql"
		)
		{
			return fluentHttpClient.CreateRequest()
				.WithUri(endPoint)
				.AsPost()
				.WithBody(query);
		}

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


		/// <summary>
		/// Send request and return as Graph QL response
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns>Returns content within Graphql data response object</returns>
		public static async Task<FluentHttpResponse<GqlResponse<T>>> ReturnAsGqlResponse<T>(this FluentHttpRequestBuilder builder)
		{
			var response = await builder.ReturnAsResponse().ConfigureAwait(false);
			var genericResponse = new FluentHttpResponse<GqlResponse<T>>(response);

			genericResponse.Data = await genericResponse.Content.ReadAsAsync<GqlResponse<T>>()
				.ConfigureAwait(false);

			return genericResponse;
		}
	}
}