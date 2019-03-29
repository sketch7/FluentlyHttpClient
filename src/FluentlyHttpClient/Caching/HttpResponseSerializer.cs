using System;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace FluentlyHttpClient.Caching
{
	public interface IHttpResponseSerializer
	{
		Task<THttpResponseStore> Serialize<THttpResponseStore>(FluentHttpResponse response) where THttpResponseStore : IHttpResponseStore, new();
		Task<FluentHttpResponse> Deserialize(IHttpResponseStore item);
	}

	public class HttpResponseSerializer : IHttpResponseSerializer
	{
		/// <summary>
		/// Convert to a serializable object.
		/// </summary>
		/// <param name="response"></param>
		/// <returns></returns>
		public async Task<THttpResponseStore> Serialize<THttpResponseStore>(FluentHttpResponse response) where THttpResponseStore : IHttpResponseStore, new()
		{
			var message = new THttpResponseStore
			{
				Content = await response.Content.ReadAsStringAsync()
			};

			message.ContentHeaders = new FluentHttpHeaders(response.Content.Headers);
			message.Hash = response.GetRequestHash();
			message.ReasonPhrase = response.ReasonPhrase;
			message.StatusCode = (int)response.StatusCode;
			message.Url = response.Message.RequestMessage.RequestUri.ToString();
			message.Version = response.Message.Version.ToString();
			message.Headers = new FluentHttpHeaders(response.Headers);
			message.RequestMessage = response.Message.RequestMessage;
			message.RequestMessage.Content = null; // deserializing cause issues so clear it.. or else if we really need it we can separate to object and reconstruct it "manually"
			return message;
		}

		/// <summary>
		/// Deserialize back to response.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public Task<FluentHttpResponse> Deserialize(IHttpResponseStore item)
		{
			var contentType = new ContentType(item.ContentHeaders.ContentType);
			var encoding = string.IsNullOrEmpty(contentType.CharSet) ? Encoding.UTF8 : Encoding.GetEncoding(contentType.CharSet);

			var cloned = new FluentHttpResponse(new HttpResponseMessage((HttpStatusCode)item.StatusCode)
			{
				Content = new StringContent(item.Content, encoding, contentType.MediaType),
				ReasonPhrase = item.ReasonPhrase,
				Version = new Version(item.Version),
				RequestMessage = item.RequestMessage,
			}); // todo: add items?

			cloned.Headers.AddRange(item.Headers);

			return Task.FromResult(cloned);
		}

	}
}