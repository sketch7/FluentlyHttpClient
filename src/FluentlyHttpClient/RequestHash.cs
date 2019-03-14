using System;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;

namespace FluentlyHttpClient
{
	public static class HashingExtensions
	{
		private const string HashKey = "REQUEST_HASH";
		private const string HashOptionsKey = "REQUEST_HASH_OPTIONS";

		/// <summary>
		/// Set request hash.
		/// </summary>
		/// <param name="message">Message instance.</param>
		/// <param name="value">Timespan value.</param>
		public static IFluentHttpMessageState SetRequestHash(this IFluentHttpMessageState message, string value)
		{
			message.Items.Add(HashKey, value);
			return message;
		}

		/// <summary>
		/// Get request hash.
		/// </summary>
		/// <param name="request">Request to get hash from.</param>
		/// <returns>Returns hash string for the request.</returns>
		public static string GetRequestHash(this FluentHttpRequest request)
		{
			if (request.Items.TryGetValue(HashKey, out var value))
				return (string)value;

			var valueStr = request.GenerateHash();
			request.SetRequestHash(valueStr);
			return valueStr;
		}

		/// <summary>
		/// Get request hash.
		/// </summary>
		/// <param name="response">Response to get hash from.</param>
		/// <returns>Returns hash string for the request.</returns>
		public static string GetRequestHash(this FluentHttpResponse response)
		{
			if (response.Items.TryGetValue(HashKey, out var value))
				return (string)value;
			return null;
		}

		/// <summary>
		/// Generate request hash.
		/// </summary>
		/// <param name="request">Request to generate hash for.</param>
		public static string GenerateHash(this FluentHttpRequest request)
		{
			var options = request.GetRequestHashOptions();
			Action<FluentHttpHeadersOptions> headersOptions = null;
			if (options?.HeadersExclude != null)
				headersOptions = httpHeadersOptions => httpHeadersOptions.HashingExclude = options.HeadersExclude;

			var headers = new FluentHttpHeaders(request.Builder.DefaultHeaders, headersOptions)
				.AddRange(request.Headers);

			var urlHash = request.Uri.IsAbsoluteUri
				? request.Uri
				: new Uri($"{request.Builder.BaseUrl.TrimEnd('/')}/{request.Uri.ToString().TrimStart('/')}");

			var headersHash = headers.ToHashString();

			var hash = $"method={request.Method};url={urlHash};headers={headersHash}";
			return hash;
		}

		/// <summary>
		/// Set request hash options for the request.
		/// </summary>
		/// <param name="requestBuilder">Request builder instance.</param>
		/// <param name="configure">Action to configure options.</param>
		public static FluentHttpRequestBuilder WithRequestHashOptions(this FluentHttpRequestBuilder requestBuilder, Action<RequestHashOptions> configure)
		{
			RequestHashOptions options;
			if (requestBuilder.Items.TryGetValue(HashOptionsKey, out var result))
				options = (RequestHashOptions)result;
			else
				options = new RequestHashOptions();

			configure?.Invoke(options);
			requestBuilder.Items[HashOptionsKey] = options;
			return requestBuilder;
		}

		/// <summary>
		/// Get response caching options for the request.
		/// </summary>
		/// <param name="request">Request to get options from.</param>
		public static RequestHashOptions GetRequestHashOptions(this FluentHttpRequest request)
		{
			request.Items.TryGetValue(HashOptionsKey, out var result);
			return (RequestHashOptions)result;
		}
	}

	public class RequestHashOptions
	{
		/// <summary>
		/// Predicate function to exclude headers from being hashed in <see cref="FluentHttpHeaders.ToHashString"/>.
		/// </summary>
		public Predicate<KeyValuePair<string, StringValues>> HeadersExclude { get; private set; }

		public RequestHashOptions WithHeadersExclude(Predicate<KeyValuePair<string, StringValues>> predicate, bool replace = false)
		{
			if (replace)
				HeadersExclude = predicate;
			else
			{
				var headersExclude = HeadersExclude;
				if (headersExclude == null)
					HeadersExclude = predicate;
				else
					HeadersExclude = p => headersExclude(p) || predicate(p);
			}

			return this;
		}
	}
}
