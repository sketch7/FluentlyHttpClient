using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using Microsoft.Extensions.Primitives;

namespace FluentlyHttpClient
{
	/// <summary>
	/// <see cref="FluentHttpHeaders"/> options.
	/// </summary>
	public class FluentHttpHeadersOptions
	{
		/// <summary>
		/// Predicate function to exclude headers from being hashed in <see cref="FluentHttpHeaders.ToHashString"/>.
		/// </summary>
		public Predicate<KeyValuePair<string, string[]>> HashingExclude { get; private set; }

		/// <summary>
		/// Add headers exclude filtering (it will be combined).
		/// </summary>
		/// <param name="predicate">Predicate to add for excluding headers.</param>
		/// <param name="replace">Determine whether to replace instead of combine.</param>
		/// <returns>When true is returned header will be filtered.</returns>
		public FluentHttpHeadersOptions WithHashingExclude(Predicate<KeyValuePair<string, string[]>> predicate, bool replace = false)
		{
			if (replace)
				HashingExclude = predicate;
			else
			{
				var headersExclude = HashingExclude;
				if (headersExclude == null)
					HashingExclude = predicate;
				else
					HashingExclude = p => headersExclude(p) || predicate(p);
			}

			return this;
		}
	}

	/// <summary>
	/// Collection of headers and their values.
	/// </summary>
	public partial class FluentHttpHeaders : IEnumerable<KeyValuePair<string, string[]>>
	{
		private static readonly FluentHttpHeadersOptions DefaultOptions = new FluentHttpHeadersOptions();
		private FluentHttpHeadersOptions _options = DefaultOptions;
		public readonly Dictionary<string, string[]> _data = new Dictionary<string, string[]>();

		public string[] this[string key]
		{
			get => _data[key];
			set => _data[key] = value;
		}

		/// <summary>
		/// Gets the headers count.
		/// </summary>
		public int Count => _data.Count;

		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		public FluentHttpHeaders()
		{
		}

		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		public FluentHttpHeaders(IEnumerable<KeyValuePair<string, string[]>> headers)
		{
			AddRange(headers);
		}

		/// <summary>
		/// Initializes a new instance with specified headers.
		/// </summary>
		/// <param name="headers">Headers to initialize with.</param>
		public FluentHttpHeaders(IDictionary<string, string[]> headers)
		{
			AddRange(headers);
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
		/// Add single header.
		/// </summary>
		/// <param name="key">Header to add.</param>
		/// <param name="value">Header value to add.</param>
		public FluentHttpHeaders Add(string key, string value)
		{
			_data.Add(key, new[] { value });
			return this;
		}

		/// <summary>
		/// Add single header.
		/// </summary>
		/// <param name="key">Header to add.</param>
		/// <param name="value">Header value to add.</param>
		public FluentHttpHeaders Add(string key, string[] value)
		{
			_data.Add(key, value);
			return this;
		}

		/// <summary>
		/// Add range and throws if already exists.
		/// </summary>
		/// <param name="headers">Headers to add from.</param>
		public FluentHttpHeaders AddRange(IEnumerable<KeyValuePair<string, string[]>> headers)
		{
			foreach (var header in headers)
				_data.Add(header.Key, header.Value);
			return this;
		}

		/// <summary>
		/// Add range and throws if already exists.
		/// </summary>
		/// <param name="headers">Headers to add from.</param>
		public FluentHttpHeaders AddRange(IDictionary<string, string[]> headers)
		{
			foreach (var header in headers)
				_data.Add(header.Key, header.Value);
			return this;
		}

		/// <summary>
		/// Add range and throws if already exists.
		/// </summary>
		/// <param name="headers">Headers to add from.</param>
		public FluentHttpHeaders AddRange(IDictionary<string, IEnumerable<string>> headers)
		{
			foreach (var header in headers)
				_data.Add(header.Key, header.Value.ToArray());
			return this;
		}

		/// <summary>
		/// Add range and throws if already exists.
		/// </summary>
		/// <param name="headers">Headers to add from.</param>
		public FluentHttpHeaders AddRange(IDictionary<string, string> headers)
		{
			foreach (var header in headers)
				_data.Add(header.Key, new[] { header.Value });
			return this;
		}

		/// <summary>
		/// Add range and throws if already exists.
		/// </summary>
		/// <param name="headers">Headers to add from.</param>
		public FluentHttpHeaders AddRange(IDictionary<string, StringValues> headers)
		{
			foreach (var header in headers)
				Add(header.Key, header.Value.ToArray());
			return this;
		}

		/// <summary>
		/// Add range and throws if already exists.
		/// </summary>
		/// <param name="headers">Headers to add from.</param>
		public FluentHttpHeaders AddRange(HttpHeaders headers)
		{
			foreach (var header in headers)
				_data.Add(header.Key, header.Value.ToArray());
			return this;
		}

		/// <summary>
		/// Get header by key or return null.
		/// </summary>
		/// <param name="header">Header to try get.</param>
		public StringValues Get(string header)
		{
			_data.TryGetValue(header, out var value);
			return value;
		}

		/// <summary>
		/// Get header as string by key or return null.
		/// </summary>
		/// <param name="header">Header to try get.</param>
		public string GetValue(string header)
			=> _data.TryGetValue(header, out var value) ? value[0] : null;

		/// <summary>
		/// Set single header add/update if exists instead of throwing.
		/// </summary>
		/// <param name="key">Header to add.</param>
		/// <param name="value">Header value to add.</param>
		public FluentHttpHeaders Set(string key, string value)
		{
			this[key] = new[] { value };
			return this;
		}

		/// <summary>
		/// Set single header add/update if exists instead of throwing.
		/// </summary>
		/// <param name="key">Header to add.</param>
		/// <param name="value">Header value to add.</param>
		public FluentHttpHeaders Set(string key, string[] value) 
		{
			this[key] = value;
			return this;
		}

		/// <summary>
		/// Set range add/update if exists instead of throwing.
		/// </summary>
		/// <param name="headers">Headers to set from.</param>
		public FluentHttpHeaders SetRange(IDictionary<string, IEnumerable<string>> headers)
		{
			foreach (var header in headers)
				this[header.Key] = header.Value.ToArray();
			return this;
		}

		/// <summary>
		/// Set range add/update if exists instead of throwing.
		/// </summary>
		/// <param name="headers">Headers to set from.</param>
		public FluentHttpHeaders SetRange(IDictionary<string, string> headers)
		{
			foreach (var header in headers)
				Set(header.Key, header.Value);
			return this;
		}

		/// <summary>
		/// Set range add/update if exists instead of throwing.
		/// </summary>
		/// <param name="headers">Headers to set from.</param>
		public FluentHttpHeaders SetRange(IDictionary<string, string[]> headers)
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
		public FluentHttpHeaders SetRange(FluentHttpHeaders headers)
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
				this[header.Key] = header.Value.ToArray();
			return this;
		}

		/// <summary>
		/// Configure header options.
		/// </summary>
		/// <param name="configure">Configure options action.</param>
		public FluentHttpHeaders WithOptions(Action<FluentHttpHeadersOptions> configure)
		{
			if (_options == DefaultOptions)
				_options = new FluentHttpHeadersOptions();
			configure(_options);

			return this;
		}

		/// <summary>
		/// Converts headers to hash string.
		/// </summary>
		public string ToHashString()
		{
			var headers = (IEnumerable<KeyValuePair<string, string[]>>)_data;

			if (_options.HashingExclude != null)
				headers = headers.Where(x => !_options.HashingExclude(x));

			var headersHash = "";
			foreach (var header in headers)
				headersHash += $"{header.Key}={string.Join(",", header.Value)}&";
			headersHash = headersHash.TrimEnd('&');
			return headersHash;
		}

		public IEnumerator<KeyValuePair<string, string[]>> GetEnumerator() => _data.GetEnumerator();

		/// <summary>
		/// Converts to string.
		/// </summary>
		public override string ToString() => ToHashString();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public Dictionary<string, string[]> ToDictionary() => _data;
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
			set => this[HeaderTypes.Authorization] = new[] { value };
		}

		/// <summary>
		/// Gets or sets the Cache-Control header.
		/// </summary>
		public string CacheControl
		{
			get => Get(HeaderTypes.CacheControl);
			set => this[HeaderTypes.CacheControl] = new[] { value };
		}

		/// <summary>
		/// Gets or sets the Content-Type header.
		/// </summary>
		public string ContentType
		{
			get => Get(HeaderTypes.ContentType);
			set => this[HeaderTypes.ContentType] = new[] { value };
		}

		/// <summary>
		/// Gets or sets the User-Agent header.
		/// </summary>
		public string UserAgent
		{
			get => Get(HeaderTypes.UserAgent);
			set => this[HeaderTypes.UserAgent] = new[] { value };
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
			set => this[HeaderTypes.XForwardedHost] = new[] { value };
		}
	}
}