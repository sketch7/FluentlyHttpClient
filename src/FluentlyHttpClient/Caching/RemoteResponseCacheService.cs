using System;
using System.Collections.Generic;
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
		public Dictionary<string, string> Headers { get; set; }
		public int StatusCode { get; set; }
		public string ReasonPhrase { get; set; }
		public string Version { get; set; }
		public Dictionary<string, string> ContentHeaders { get; set; }
		public HttpRequestMessage RequestMessage { get; set; }
	}

	public class HttpResponseSerializer
	{
		/// <summary>
		/// Convert to a serializable object.
		/// </summary>
		/// <param name="response"></param>
		/// <returns></returns>
		public async Task<MessageItemStore> Serialize(FluentHttpResponse response)
		{
			var message = new MessageItemStore();

			var contentString = await response.Content.ReadAsStringAsync();

			message.Content = contentString;
			message.ContentHeaders = response.Content.Headers.ToDictionary();
			//message.ContentHeaders = JsonConvert.SerializeObject(response.Content.Headers.ToDictionary());
			message.Hash = response.GetRequestHash();
			message.ReasonPhrase = response.ReasonPhrase;
			message.StatusCode = (int)response.StatusCode;
			message.Url = response.Message.RequestMessage.RequestUri.ToString();
			message.Version = response.Message.Version.ToString();
			message.Headers = response.Headers.ToDictionary();
			//message.Headers = JsonConvert.SerializeObject(response.Headers.ToDictionary());
			message.RequestMessage = response.Message.RequestMessage;
			//message.RequestMessage = JsonConvert.SerializeObject(response.Message.RequestMessage);

			return message;
		}

		/// <summary>
		/// Deserialize back to response.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public Task<FluentHttpResponse> Deserialize(MessageItemStore item)
		{
			//var headersMap = JsonConvert.DeserializeObject<Dictionary<string, string>>(item.Headers);
			//var contentHeadersMap = JsonConvert.DeserializeObject<Dictionary<string, string>>(item.ContentHeaders);
			//var requestMessage = JsonConvert.DeserializeObject<HttpRequestMessage>(item.RequestMessage);

			//var contentType = new ContentType(contentHeadersMap["Content-Type"]);
			var contentType = new ContentType(item.ContentHeaders["Content-Type"]);
			var encoding = Encoding.GetEncoding(contentType.CharSet);

			var cloned = new FluentHttpResponse(new HttpResponseMessage((HttpStatusCode)item.StatusCode)
			{
				Content = new StringContent(item.Content, encoding, contentType.MediaType),
				ReasonPhrase = item.ReasonPhrase,
				Version = new Version(item.Version),
				//RequestMessage = requestMessage,
				RequestMessage = item.RequestMessage,
			}); // todo: add items?

			cloned.Headers.AddRange(item.Headers);
			//cloned.Headers.AddRange(headersMap);

			return Task.FromResult(cloned);
		}

	}


	public class RemoteResponseCacheService : IResponseCacheService
	{
		private readonly Dictionary<string, FluentHttpResponse> _cache = new Dictionary<string, FluentHttpResponse>();

		public Task<FluentHttpResponse> Get(string hash, FluentHttpRequest request)
		{
			// todo: get from db
			throw new NotImplementedException();
		}

		public async Task Set(string hash, FluentHttpResponse response)
		{
			// todo: insert in DB
			throw new NotImplementedException();
		}

	}
}
