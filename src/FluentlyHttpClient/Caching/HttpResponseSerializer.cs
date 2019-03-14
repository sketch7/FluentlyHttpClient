using System;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace FluentlyHttpClient.Caching
{
	public class MessageItemStore
	{
		public string Name { get; set; }
		public string Hash { get; set; }
		public string Url { get; set; }
		public string Content { get; set; }
		public FluentHttpHeaders Headers { get; set; }
		public int StatusCode { get; set; }
		public string ReasonPhrase { get; set; }
		public string Version { get; set; }
		public FluentHttpHeaders ContentHeaders { get; set; }
		public HttpRequestMessage RequestMessage { get; set; }
	}


	public interface IHttpResponseSerializer
	{
		Task<MessageItemStore> Serialize(FluentHttpResponse response);
		Task<FluentHttpResponse> Deserialize(MessageItemStore item);
	}

	public class HttpResponseSerializer : IHttpResponseSerializer
	{
		/// <summary>
		/// Convert to a serializable object.
		/// </summary>
		/// <param name="response"></param>
		/// <returns></returns>
		public async Task<MessageItemStore> Serialize(FluentHttpResponse response)
		{
			var message = new MessageItemStore
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

			return message;
		}

		/// <summary>
		/// Deserialize back to response.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public Task<FluentHttpResponse> Deserialize(MessageItemStore item)
		{
			var contentType = new ContentType(item.ContentHeaders["Content-Type"]);
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