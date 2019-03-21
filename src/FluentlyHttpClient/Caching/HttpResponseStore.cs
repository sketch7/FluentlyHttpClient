using System.Net.Http;

namespace FluentlyHttpClient.Caching
{
	public interface IHttpResponseStore
	{
		string Name { get; set; }
		string Hash { get; set; }
		string Url { get; set; }
		string Content { get; set; }
		FluentHttpHeaders Headers { get; set; }
		int StatusCode { get; set; }
		string ReasonPhrase { get; set; }
		string Version { get; set; }
		FluentHttpHeaders ContentHeaders { get; set; }
		HttpRequestMessage RequestMessage { get; set; }
	}

	public class HttpResponseStore : IHttpResponseStore
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
}