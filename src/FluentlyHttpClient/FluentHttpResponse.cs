using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace FluentlyHttpClient
{
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

		/// <summary>
		/// Initializes a new <see cref="FluentHttpResponse"/>.
		/// </summary>
		/// <param name="response"></param>
		public FluentHttpResponse(FluentHttpResponse response) : base(response.Message)
		{
			Items = response.Items;
		}

		/// <summary>
		/// Gets readable response info as string.
		/// </summary>
		public override string ToString() => $"{DebuggerDisplay}";
	}

	/// <summary>
	/// Fluent HTTP response, which wraps the <see cref="HttpResponseMessage"/> and add additional features.
	/// </summary>
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class FluentHttpResponse : IFluentHttpMessageState
	{
		/// <summary>
		/// Gets readable string for debugger.
		/// </summary>
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
		/// Gets or sets the reason phrase which typically is sent by the server together with the status code.
		/// </summary>
		public string ReasonPhrase
		{
			get => Message.ReasonPhrase;
			set => Message.ReasonPhrase = value;
		}

		/// <summary>
		/// Gets or sets the content of HTTP response.
		/// </summary>
		public HttpContent Content
		{
			get => Message.Content;
			set => Message.Content = value;
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
		/// Gets the collection of HTTP response headers.
		/// </summary>
		public HttpResponseHeaders Headers => Message.Headers;

		/// <inheritdoc />
		public IDictionary<object, object> Items { get; protected set; }

		/// <summary>
		/// Gets readable response info as string.
		/// </summary>
		public override string ToString() => $"{DebuggerDisplay}";

		/// <summary>
		/// Initializes a new <see cref="FluentHttpResponse"/>.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="items"></param>
		public FluentHttpResponse(HttpResponseMessage message, IDictionary<object, object> items = null)
		{
			Message = message;
			Items = items ?? new Dictionary<object, object>();
		}
	}
}