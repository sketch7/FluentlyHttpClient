using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace FluentlyHttpClient
{
	/// <summary>
	/// Fluent Http Client extensions.
	/// </summary>
	public static class FluentHttpClientExtensions
	{
		/// <summary>
		/// Create and send a HTTP GET request and return specified <see cref="T"/> as result.
		/// </summary>
		/// <typeparam name="T">Type to deserialize content to.</typeparam>
		/// <param name="client">http client instance.</param>
		/// <param name="uri">Request resource uri to send.</param>
		/// <returns>Returns task with the result data.</returns>
		public static Task<T> Get<T>(this FluentHttpClient client, string uri) => client.CreateRequest(uri)
			.AsGet()
			.WithSuccessStatus()
			.Return<T>();

		/// <summary>
		/// Create and send a HTTP POST request and return specified <see cref="T"/> as result.
		/// </summary>
		/// <typeparam name="T">Type to deserialize content to.</typeparam>
		/// <param name="client">http client instance.</param>
		/// <param name="uri">Request resource uri to send.</param>
		/// <param name="data">Payload data content to send.</param>
		/// <param name="contentType">(Optional) content type to use when sending data.</param>
		/// <returns>Returns task with the result data.</returns>
		public static Task<T> Post<T>(this FluentHttpClient client, string uri, object data, MediaTypeHeaderValue contentType = null)
			=> client.CreateRequest(uri)
			.AsPost()
			.WithSuccessStatus()
			.WithBody(data, contentType)
			.Return<T>();

		/// <summary>
		/// Create and send a HTTP PUT request and return specified <see cref="T"/> as result.
		/// </summary>
		/// <typeparam name="T">Type to deserialize content to.</typeparam>
		/// <param name="client">http client instance.</param>
		/// <param name="uri">Request resource uri to send.</param>
		/// <param name="data">Payload data content to send.</param>
		/// <param name="contentType">(Optional) content type to use when sending data.</param>
		/// <returns>Returns task with the result data.</returns>
		public static Task<T> Put<T>(this FluentHttpClient client, string uri, object data, MediaTypeHeaderValue contentType = null)
			=> client.CreateRequest(uri)
			.AsPut()
			.WithSuccessStatus()
			.WithBody(data, contentType)
			.Return<T>();

		/// <summary>
		/// Create and send a HTTP PATCH request and return specified <see cref="T"/> as result.
		/// </summary>
		/// <typeparam name="T">Type to deserialize content to.</typeparam>
		/// <param name="client">http client instance</param>
		/// <param name="uri">Request resource uri to send.</param>
		/// <param name="data">Payload data content to send.</param>
		/// <param name="contentType">(Optional) content type to use when sending data.</param>
		/// <returns>Returns task with the result data.</returns>
		public static Task<T> Patch<T>(this FluentHttpClient client, string uri, object data, MediaTypeHeaderValue contentType = null)
			=> client.CreateRequest(uri)
			.AsPatch()
			.WithSuccessStatus()
			.WithBody(data, contentType)
			.Return<T>();

		/// <summary>
		/// Create and send a HTTP DELETE request and return specified <see cref="T"/> as result.
		/// </summary>
		/// <typeparam name="T">Type to deserialize content to.</typeparam>
		/// <param name="client">http client instance</param>
		/// <param name="uri">Request resource uri to send.</param>
		/// <param name="data">Payload data content to send.</param>
		/// <param name="contentType">(Optional) content type to use when sending data.</param>
		/// <returns>Returns task with the result data.</returns>
		public static Task<T> Delete<T>(this FluentHttpClient client, string uri, object data, MediaTypeHeaderValue contentType = null) 
			=> client.CreateRequest(uri)
			.AsDelete()
			.WithSuccessStatus()
			.WithBody(data, contentType)
			.Return<T>();

	}
}