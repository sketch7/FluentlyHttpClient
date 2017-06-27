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
	/// <param name="request"></param>
	/// <returns></returns>
	public delegate Task<FluentHttpResponse> FluentHttpRequestDelegate(FluentHttpRequest request);

	/// <summary>
	/// Fluent http request, which wraps the <see cref="HttpRequestMessage"/> and add additional features.
	/// </summary>
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class FluentHttpRequest
	{
		private string DebuggerDisplay => $"[{Method}] '{Url}'";

		/// <summary>
		/// Gets the Raw <see cref="HttpRequestMessage"/>.
		/// </summary>
		public HttpRequestMessage RawRequest { get; }

		/// <summary>
		/// Gets the <see cref="HttpMethod"/> for the HTTP request.
		/// </summary>
		public HttpMethod Method => RawRequest.Method;

		/// <summary>
		/// Gets the <see cref="Uri"/> for the HTTP request.
		/// </summary>
		public Uri Url => RawRequest.RequestUri;

		/// <summary>
		/// Gets the collection of HTTP request headers.
		/// </summary>
		public HttpRequestHeaders Headers => RawRequest.Headers;
		
		/// <summary>
		/// Determine whether has success status otherwise it will throw or not.
		/// </summary>
		public bool HasSuccessStatusOrThrow { get; set; }

		/// <summary>
		/// Cancellation token to cancel operation.
		/// </summary>
		public CancellationToken CancellationToken { get; set; }

		public FluentHttpRequest(HttpRequestMessage rawRequest)
		{
			RawRequest = rawRequest;
		}

		public override string ToString() => $"{DebuggerDisplay}";
	}

	/// <summary>
	/// Fluent Http response, which wraps the <see cref="FluentHttpResponse"/> and add data.
	/// </summary>
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class FluentHttpResponse<T> : FluentHttpResponse
	{
		/// <summary>
		/// Data content.
		/// </summary>
		public T Data { get; set; }

		public FluentHttpResponse(FluentHttpResponse response) : base(response.RawResponse)
		{
			Items = response.Items;
		}

		public override string ToString() => $"{DebuggerDisplay}";
	}

	/// <summary>
	/// Fluent Http response, which wraps the <see cref="HttpResponseMessage"/> and add additional features.
	/// </summary>
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class FluentHttpResponse
	{
		protected string DebuggerDisplay => $"[{(int)StatusCode}] '{ReasonPhrase}', Request: {{ [{RawResponse.RequestMessage.Method}] '{RawResponse.RequestMessage.RequestUri}' }}";

		/// <summary>
		/// Http raw response.
		/// </summary>
		public HttpResponseMessage RawResponse { get; }

		/// <summary>
		/// Gets the status code of the HTTP response.
		/// </summary>
		public HttpStatusCode StatusCode => RawResponse.StatusCode;

		/// <summary>
		/// Determine whether the HTTP response was successful.
		/// </summary>
		public bool IsSuccessStatusCode => RawResponse.IsSuccessStatusCode;

		/// <summary>
		/// Throws an exception if the <see cref="IsSuccessStatusCode"/> is set to false.
		/// </summary>
		public void EnsureSuccessStatusCode() => RawResponse.EnsureSuccessStatusCode();

		/// <summary>
		/// Gets the reason phrase which typically is sent by the server together with the status code.
		/// </summary>
		public string ReasonPhrase => RawResponse.ReasonPhrase;

		/// <summary>
		/// Gets the collection of HTTP response headers.
		/// </summary>
		public HttpResponseHeaders Headers => RawResponse.Headers;

		/// <summary>
		/// Gets or sets a key/value collection that can be used to share data within the scope of request/response.
		/// </summary>
		public IDictionary<object, object> Items { get; set; } = new Dictionary<object, object>();

		public override string ToString() => $"{DebuggerDisplay}";

		public FluentHttpResponse(HttpResponseMessage rawResponse)
		{
			RawResponse = rawResponse;
		}


	}

	
}
