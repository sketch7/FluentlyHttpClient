using System;
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
		/// <returns></returns>
		public static async Task<FluentHttpResponse> Clone(this FluentHttpResponse response)
		{
			var contentString = await response.Content.ReadAsStringAsync();
			var contentType = response.Content.Headers.ContentType;
			var encoding = Encoding.GetEncoding(contentType.CharSet);

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
		/// <returns></returns>
		public static string GenerateHash(this FluentHttpRequest request)
		{
			var headers = request.Builder.DefaultHeaders.ToDictionary();
			foreach (var requestHeader in request.Headers)
				headers[requestHeader.Key] = string.Join(";", requestHeader.Value);

			var urlHash = request.Uri.IsAbsoluteUri
				? request.Uri
				: new Uri($"{request.Builder.BaseUrl.TrimEnd('/')}/{request.Uri.ToString().TrimStart('/')}");
			var headersHash = headers.ToHeadersHashString();

			var hash = $"method={request.Method};url={urlHash};headers={headersHash}";
			return hash;
		}
	}
}
