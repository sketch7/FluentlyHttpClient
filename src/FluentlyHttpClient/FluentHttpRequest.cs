using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace FluentlyHttpClient
{
	/// <summary>
	/// Delegate which is mainly used by Middleware.
	/// </summary>
	/// <param name="request">HTTP request to send.</param>
	/// <returns>Returns async response.</returns>
	public delegate Task<FluentHttpResponse> FluentHttpRequestDelegate(FluentHttpRequest request);

	/// <summary>
	/// Fluent HTTP request, which wraps the <see cref="HttpRequestMessage"/> and add additional features.
	/// </summary>
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class FluentHttpRequest
	{
		private string DebuggerDisplay => $"[{Method}] '{Uri}'";

		/// <summary>
		/// Gets the underlying HTTP request message.
		/// </summary>
		public HttpRequestMessage Message { get; }

		/// <summary>
		/// Gets or sets the <see cref="HttpMethod"/> for the HTTP request.
		/// </summary>
		public HttpMethod Method
		{
			get => Message.Method;
			set => Message.Method = value;
		}

	/// <summary>
	/// Gets or sets the <see cref="System.Uri"/> for the HTTP request.
	/// </summary>
	public Uri Uri
		{
			get => Message.RequestUri;
			set => Message.RequestUri = value;
		}

	/// <summary>
	/// Gets the collection of HTTP request headers.
	/// </summary>
	public HttpRequestHeaders Headers => Message.Headers;
		
		/// <summary>
		/// Determine whether has success status otherwise it will throw or not.
		/// </summary>
		public bool HasSuccessStatusOrThrow { get; set; }

		/// <summary>
		/// Cancellation token to cancel operation.
		/// </summary>
		public CancellationToken CancellationToken { get; set; }

		/// <summary>
		/// Gets or sets a key/value collection that can be used to share data within the scope of request/response.
		/// </summary>
		public IDictionary<object, object> Items { get; protected set; }

		/// <summary>
		/// Formatters to be used for content negotiation for "Accept" and also sending formats. e.g. (JSON, XML)
		/// </summary>
		public MediaTypeFormatterCollection Formatters { get; set; }

		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		public FluentHttpRequest(HttpRequestMessage message, IDictionary<object, object> items = null)
		{
			Message = message;
			Items = items ?? new Dictionary<object, object>();
		}

		/// <summary>
		/// Gets readable request info as string.
		/// </summary>
		public override string ToString() => $"{DebuggerDisplay}";
	}
}