using System;
using System.Collections.Generic;
using System.Net.Http;

namespace FluentlyHttpClient
{
	/// <summary>
	/// Options for <see cref="IFluentHttpClient"/>.
	/// </summary>
	public class FluentHttpClientOptions
	{
		/// <summary>
		/// Gets or sets the base uri address for each request.
		/// </summary>
		public string BaseUrl { get; set; }

		/// <summary>
		/// Gets or sets the timespan to wait before the request times out.
		/// </summary>
		public TimeSpan Timeout { get; set; }

		/// <summary>
		/// Gets or sets the identifier (key) for the http client.
		/// </summary>
		public string Identifier { get; set; }

		/// <summary>
		/// Gets or sets the headers which should be sent with each request.
		/// </summary>
		public Dictionary<string, string> Headers { get; set; }

		/// <summary>
		/// Gets or sets the middleware to be used for each request.
		/// </summary>
		public List<Type> Middleware { get; set; }

		/// <summary>
		/// Handler to customize request on creation. In order to specify defaults as desired, or so.
		/// </summary>
		public Action<FluentHttpRequestBuilder> RequestBuilderDefaults { get; set; }

		/// <summary>
		/// HTTP handler stack to use for sending requests.
		/// </summary>
		public HttpMessageHandler HttpMessageHandler { get; set; }
	}
}