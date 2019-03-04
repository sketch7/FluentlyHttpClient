using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using Microsoft.Extensions.Primitives;

namespace FluentlyHttpClient
{
	/// <summary>
	/// Collection of headers and their values.
	/// </summary>
	public partial class FluentHttpHeaders : Dictionary<string, StringValues>
	{
		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		public FluentHttpHeaders()
		{
		}

		/// <summary>
		/// Initializes a new instance with specified headers.
		/// </summary>
		/// <param name="headers">Headers to initialize with.</param>
		public FluentHttpHeaders(IDictionary<string, IEnumerable<string>> headers)
		{
			AddRange(headers);
		}

		/// <summary>
		/// Initializes a new instance with specified headers.
		/// </summary>
		/// <param name="headers">Headers to initialize with.</param>
		public FluentHttpHeaders(IDictionary<string, string> headers)
		{
			AddRange(headers);
		}

		/// <summary>
		/// Initializes a new instance with specified headers.
		/// </summary>
		/// <param name="headers">Headers to initialize with.</param>
		public FluentHttpHeaders(HttpHeaders headers)
		{
			AddRange(headers);
		}

		/// <summary>
		/// Add range and throws if already exists.
		/// </summary>
		/// <param name="headers">Headers to add from.</param>
		public FluentHttpHeaders AddRange(IDictionary<string, IEnumerable<string>> headers)
		{
			foreach (var header in headers)
				Add(header.Key, new StringValues(header.Value.ToArray()));
			return this;
		}

		/// <summary>
		/// Add range and throws if already exists.
		/// </summary>
		/// <param name="headers">Headers to add from.</param>
		public FluentHttpHeaders AddRange(IDictionary<string, string> headers)
		{
			foreach (var header in headers)
				Add(header.Key, header.Value);
			return this;
		}

		/// <summary>
		/// Add range and throws if already exists.
		/// </summary>
		/// <param name="headers">Headers to add from.</param>
		public FluentHttpHeaders AddRange(IDictionary<string, StringValues> headers)
		{
			foreach (var header in headers)
				Add(header.Key, header.Value);
			return this;
		}

		/// <summary>
		/// Add range and throws if already exists.
		/// </summary>
		/// <param name="headers">Headers to add from.</param>
		public FluentHttpHeaders AddRange(HttpHeaders headers)
		{
			foreach (var header in headers)
				Add(header.Key, new StringValues(header.Value.ToArray()));
			return this;
		}

		/// <summary>
		/// Get header by key or return null.
		/// </summary>
		/// <param name="header">Header to try get.</param>
		public StringValues Get(string header)
		{
			TryGetValue(header, out var value);
			return value;
		}

		/// <summary>
		/// Set range add/update if exists instead of throwing.
		/// </summary>
		/// <param name="headers">Headers to set from.</param>
		public FluentHttpHeaders SetRange(IDictionary<string, IEnumerable<string>> headers)
		{
			foreach (var header in headers)
				this[header.Key] = new StringValues(header.Value.ToArray());
			return this;
		}

		/// <summary>
		/// Set range add/update if exists instead of throwing.
		/// </summary>
		/// <param name="headers">Headers to set from.</param>
		public FluentHttpHeaders SetRange(IDictionary<string, string> headers)
		{
			foreach (var header in headers)
				this[header.Key] = header.Value;
			return this;
		}

		/// <summary>
		/// Set range add/update if exists instead of throwing.
		/// </summary>
		/// <param name="headers">Headers to set from.</param>
		public FluentHttpHeaders SetRange(IDictionary<string, StringValues> headers)
		{
			foreach (var header in headers)
				this[header.Key] = header.Value;
			return this;
		}

		/// <summary>
		/// Set range add/update if exists instead of throwing.
		/// </summary>
		/// <param name="headers">Headers to set from.</param>
		public FluentHttpHeaders SetRange(HttpHeaders headers)
		{
			foreach (var header in headers)
				this[header.Key] = new StringValues(header.Value.ToArray());
			return this;
		}

		/// <summary>
		/// Converts headers to hash string.
		/// </summary>
		public string ToHashString()
		{
			var headers = this;
			var headersHash = "";
			foreach (var header in headers)
				headersHash += $"{header.Key}={header.Value}&";
			headersHash = headersHash.TrimEnd('&');
			return headersHash;
		}

		/// <summary>
		/// Converts to string.
		/// </summary>
		public override string ToString() => ToHashString();
	}

	// headers accessors
	public partial class FluentHttpHeaders
	{
		/// <summary>
		/// Gets or sets the Accept header.
		/// </summary>
		public StringValues Accept
		{
			get => Get(HeaderTypes.Accept);
			set => this[HeaderTypes.Accept] = value;
		}

		/// <summary>
		/// Gets or sets the Accept-Language header.
		/// </summary>
		public StringValues AcceptLanguage
		{
			get => Get(HeaderTypes.AcceptLanguage);
			set => this[HeaderTypes.Accept] = value;
		}

		/// <summary>
		/// Gets or sets the Authorization header.
		/// </summary>
		public string Authorization
		{
			get => Get(HeaderTypes.Authorization);
			set => this[HeaderTypes.Authorization] = value;
		}

		/// <summary>
		/// Gets or sets the Cache-Control header.
		/// </summary>
		public string CacheControl
		{
			get => Get(HeaderTypes.CacheControl);
			set => this[HeaderTypes.CacheControl] = value;
		}

		/// <summary>
		/// Gets or sets the Content-Type header.
		/// </summary>
		public string ContentType
		{
			get => Get(HeaderTypes.ContentType);
			set => this[HeaderTypes.ContentType] = value;
		}

		/// <summary>
		/// Gets or sets the User-Agent header.
		/// </summary>
		public string UserAgent
		{
			get => Get(HeaderTypes.UserAgent);
			set => this[HeaderTypes.UserAgent] = value;
		}

		/// <summary>
		/// Gets or sets the X-Forwarded-For header.
		/// </summary>
		public StringValues XForwardedFor
		{
			get => Get(HeaderTypes.XForwardedFor);
			set => this[HeaderTypes.XForwardedFor] = value;
		}

		/// <summary>
		/// Gets or sets the X-Forwarded-Host header.
		/// </summary>
		public string XForwardedHost
		{
			get => Get(HeaderTypes.XForwardedHost);
			set => this[HeaderTypes.XForwardedHost] = value;
		}
	}
}