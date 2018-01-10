using System.Net.Http;

namespace FluentlyHttpClient.GraphQL
{
	/// <summary>
	/// Fluent HTTP response, which wraps the <see cref="HttpResponseMessage"/> and adds GraphQL data.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class GqlResponse<T>
	{
		/// <summary>
		/// GraphQL content data.
		/// </summary>
		public T Data { get; }

		/// <summary>
		/// Initialize a new GqlResponse.
		/// </summary>
		/// <param name="data"></param>
		public GqlResponse(T data)
		{
			Data = data;
		}
	}
}
