﻿using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FluentlyHttpClient
{
	/// <summary>
	/// Extensions for Http Request/Response
	/// </summary>
	public static class FluentHttpMessageExtensions
	{
		/// <summary>
		/// Clone response.
		/// </summary>
		/// <param name="response">Response to clone.</param>
		public static async Task<FluentHttpResponse> Clone(this FluentHttpResponse response)
		{
			var contentString = await response.Content.ReadAsStringAsync();
			var contentType = response.Content.Headers.ContentType;
			var encoding = string.IsNullOrEmpty(contentType.CharSet) ? Encoding.UTF8 : Encoding.GetEncoding(contentType.CharSet);

			var cloned = new FluentHttpResponse(new HttpResponseMessage(response.StatusCode)
			{
				Content = new StringContent(contentString, encoding, contentType.MediaType),
				ReasonPhrase = response.ReasonPhrase,
				Version = response.Message.Version,
				RequestMessage = response.Message.RequestMessage,
			}, response.Items);

			cloned.Headers.CopyFrom(response.Headers);

			return cloned;
		}

		/// <summary>
		/// Generate request hash.
		/// </summary>
		/// <param name="request">Request to generate hash for.</param>
		public static string GenerateHash(this FluentHttpRequest request)
		{
			var headers = new FluentHttpHeaders(request.Builder.DefaultHeaders)
				.AddRange(request.Headers);

			var urlHash = request.Uri.IsAbsoluteUri
				? request.Uri
				: new Uri($"{request.Builder.BaseUrl.TrimEnd('/')}/{request.Uri.ToString().TrimStart('/')}");

			var headersHash = headers.ToHashString();

			var hash = $"method={request.Method};url={urlHash};headers={headersHash}";
			return hash;
		}
	}
}
