﻿using System;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace FluentlyHttpClient.Caching
{
	public interface IHttpResponseSerializer
	{
		Task<TMessageItemStore> Serialize<TMessageItemStore>(FluentHttpResponse response) where TMessageItemStore : IMessageItemStore, new();
		Task<FluentHttpResponse> Deserialize(IMessageItemStore item);
	}

	public class HttpResponseSerializer : IHttpResponseSerializer
	{
		/// <summary>
		/// Convert to a serializable object.
		/// </summary>
		/// <param name="response"></param>
		/// <returns></returns>
		public async Task<TMessageItemStore> Serialize<TMessageItemStore>(FluentHttpResponse response) where TMessageItemStore : IMessageItemStore, new()
		{
			var message = new TMessageItemStore
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
		public Task<FluentHttpResponse> Deserialize(IMessageItemStore item)
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