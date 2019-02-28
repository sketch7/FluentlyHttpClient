using System;
using System.Threading.Tasks;
using FluentlyHttpClient.Caching;
using FluentlyHttpClient.Middleware;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace FluentlyHttpClient.Middleware
{
	/// <summary>
	/// Request Caching HTTP middleware options.
	/// </summary>
	public class ResponseCacheHttpMiddlewareOptions
	{
		/// <summary>
		/// Ignore the request from being cached.
		/// </summary>
		public bool ShouldIgnore { get; set; }
	}

	/// <summary>
	/// Request caching middleware for HTTP client.
	/// </summary>
	public class ResponseCacheHttpMiddleware : IFluentHttpMiddleware
	{
		private readonly FluentHttpRequestDelegate _next;
		private readonly ResponseCacheHttpMiddlewareOptions _options;
		private readonly ILogger _logger;
		private readonly IResponseCacheService _requestCache;

		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		public ResponseCacheHttpMiddleware(
			FluentHttpRequestDelegate next,
			ResponseCacheHttpMiddlewareOptions options,
			ILogger<ResponseCacheHttpMiddleware> logger,
			IResponseCacheService requestCache
		)
		{
			_next = next;
			_options = options;
			_logger = logger;
			_requestCache = requestCache;
		}

		/// <inheritdoc />
		public async Task<FluentHttpResponse> Invoke(FluentHttpRequest request)
		{
			var options = request.GetRequestCachingOptions(_options);

			if (options.ShouldIgnore || !_requestCache.Matcher(request))
				return await _next(request);

			var hash = request.GetRequestHash();
			if (string.IsNullOrEmpty(hash))
			{
				hash = _requestCache.GenerateHash(request);
				request.SetRequestHash(hash);
			}
			
			var response = await _requestCache.Get(hash, request);
			if (response != null)
			{
				_logger.LogInformation("Pre-request - Returning a cached response {hash}", hash);
				return response;
			}

			response = await _next(request);

			_logger.LogInformation("Post-Response - Caching request... {hash}", hash);
			await _requestCache.Set(hash, response);

			return response;
		}
	}
}

namespace FluentlyHttpClient
{
	/// <summary>
	/// Request Caching HTTP middleware extensions.
	/// </summary>
	public static class ResponseCacheHttpMiddlewareExtensions
	{
		private const string OptionsKey = "RESPONSE_CACHE_OPTIONS";
		private const string HashKey = "RESPONSE_CACHE_HASH";

		#region Request Extensions
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
		/// <param name="message">Message to get hash from.</param>
		/// <returns>Returns hash string for the request.</returns>
		public static string GetRequestHash(this IFluentHttpMessageState message)
		{
			message.Items.TryGetValue(HashKey, out var value);
			return (string)value;
		}

		/// <summary>
		/// Set request caching options for the request.
		/// </summary>
		/// <param name="requestBuilder">Request builder instance.</param>
		/// <param name="options">Options to set.</param>
		public static FluentHttpRequestBuilder WithRequestCachingOptions(this FluentHttpRequestBuilder requestBuilder, ResponseCacheHttpMiddlewareOptions options)
		{
			requestBuilder.Items[OptionsKey] = options;
			return requestBuilder;
		}

		/// <summary>
		/// Set request caching options for the request.
		/// </summary>
		/// <param name="requestBuilder">Request builder instance.</param>
		/// <param name="configure">Action to configure options.</param>
		public static FluentHttpRequestBuilder WithRequestCachingOptions(this FluentHttpRequestBuilder requestBuilder, Action<ResponseCacheHttpMiddlewareOptions> configure)
		{
			var options = new ResponseCacheHttpMiddlewareOptions();
			configure?.Invoke(options);
			return requestBuilder.WithRequestCachingOptions(options);
		}

		/// <summary>
		/// Get logging option for the request.
		/// </summary>
		/// <param name="request">Request to get options from.</param>
		/// <param name="defaultOptions"></param>
		/// <returns>Returns merged logging options.</returns>
		public static ResponseCacheHttpMiddlewareOptions GetRequestCachingOptions(this FluentHttpRequest request, ResponseCacheHttpMiddlewareOptions defaultOptions = null)
		{
			if (!request.Items.TryGetValue(OptionsKey, out var result)) return defaultOptions;
			var options = (ResponseCacheHttpMiddlewareOptions)result;
			if (defaultOptions == null)
				return options;
			return options;
		}
		#endregion

		/// <summary>
		/// Use response caching middleware which get from cache or get from remote and cache responses.
		/// </summary>
		/// <param name="builder">Builder instance</param>
		/// <param name="options"></param>
		public static FluentHttpClientBuilder UseResponseCaching(this FluentHttpClientBuilder builder, ResponseCacheHttpMiddlewareOptions options = null)
			=> builder.UseMiddleware<ResponseCacheHttpMiddleware>(options ?? new ResponseCacheHttpMiddlewareOptions());

		/// <summary>
		/// Use response caching middleware which get from cache or get from remote and cache responses.
		/// </summary>
		/// <param name="builder">Builder instance</param>
		/// <param name="configure">Action to configure caching options.</param>
		public static FluentHttpClientBuilder UseResponseCaching(this FluentHttpClientBuilder builder, Action<ResponseCacheHttpMiddlewareOptions> configure)
		{
			var options = new ResponseCacheHttpMiddlewareOptions();
			configure?.Invoke(options);
			return builder.UseResponseCaching(options);
		}
	}
}