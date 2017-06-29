using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
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
		public IDictionary<object, object> Items { get; set; } = new Dictionary<object, object>();

		public FluentHttpRequest(HttpRequestMessage message)
		{
			Message = message;
		}

		public override string ToString() => $"{DebuggerDisplay}";
	}

	/// <summary>
	/// Fluent HTTP response, which wraps the <see cref="FluentHttpResponse"/> and add data.
	/// </summary>
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class FluentHttpResponse<T> : FluentHttpResponse
	{
		/// <summary>
		/// Content data.
		/// </summary>
		public T Data { get; set; }

		public FluentHttpResponse(FluentHttpResponse response) : base(response.Message)
		{
			Items = response.Items;
		}

		public override string ToString() => $"{DebuggerDisplay}";
	}

	/// <summary>
	/// Fluent HTTP response, which wraps the <see cref="HttpResponseMessage"/> and add additional features.
	/// </summary>
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class FluentHttpResponse
	{
		protected string DebuggerDisplay => $"[{(int)StatusCode}] '{ReasonPhrase}', Request: {{ [{Message.RequestMessage.Method}] '{Message.RequestMessage.RequestUri}' }}";

		/// <summary>
		/// Gets the underlying HTTP response message.
		/// </summary>
		public HttpResponseMessage Message { get; }

		/// <summary>
		/// Gets or sets the status code of the HTTP response.
		/// </summary>
		public HttpStatusCode StatusCode
		{
			get => Message.StatusCode;
			set => Message.StatusCode = value;
		}

	/// <summary>
	/// Determine whether the HTTP response was successful.
	/// </summary>
	public bool IsSuccessStatusCode => Message.IsSuccessStatusCode;

		/// <summary>
		/// Throws an exception if the <see cref="IsSuccessStatusCode"/> is set to false.
		/// </summary>
		public void EnsureSuccessStatusCode() => Message.EnsureSuccessStatusCode();

		/// <summary>
		/// Gets or sets the reason phrase which typically is sent by the server together with the status code.
		/// </summary>
		public string ReasonPhrase
		{
			get => Message.ReasonPhrase;
			set => Message.ReasonPhrase = value;
		}

		/// <summary>
		/// Gets the collection of HTTP response headers.
		/// </summary>
		public HttpResponseHeaders Headers => Message.Headers;

		/// <summary>
		/// Gets or sets a key/value collection that can be used to share data within the scope of request/response.
		/// </summary>
		public IDictionary<object, object> Items { get; set; }

		public override string ToString() => $"{DebuggerDisplay}";

		public FluentHttpResponse(HttpResponseMessage message, IDictionary<object, object> items = null)
		{
			Message = message;
			Items = items ?? new Dictionary<object, object>();
		}
	}
}