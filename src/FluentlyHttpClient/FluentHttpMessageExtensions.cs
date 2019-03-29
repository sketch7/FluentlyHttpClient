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
	}
}
