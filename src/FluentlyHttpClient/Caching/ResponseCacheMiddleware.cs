using System;
using System.Threading.Tasks;
using FluentlyHttpClient.Caching;
using FluentlyHttpClient.Middleware;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace FluentlyHttpClient.Middleware
{
	/// <summary>
	/// Response Caching HTTP middleware options.
	/// </summary>
	public class ResponseCacheHttpMiddlewareOptions
	{
		/// <summary>
		/// Ignore the request from being cached.
		/// </summary>
		public bool ShouldIgnore { get; set; }
		public Predicate<FluentHttpRequest> Matcher { get; set; }

		/// <summary>
		/// Don't attempt to read from the cache but write only. This is useful to start and populating data.
		/// </summary>
		public bool IsWriteOnly { get; set; }
	}

	/// <summary>
	/// Response caching middleware for HTTP client.
	/// </summary>
	public class ResponseCacheHttpMiddleware : IFluentHttpMiddleware
	{
		private readonly FluentHttpMiddlewareDelegate _next;
		private readonly ResponseCacheHttpMiddlewareOptions _options;
		private readonly ILogger _logger;
		private readonly IResponseCacheService _service;

		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		public ResponseCacheHttpMiddleware(
			FluentHttpMiddlewareDelegate next,
			FluentHttpMiddlewareClientContext context,
			ResponseCacheHttpMiddlewareOptions options,
			ILoggerFactory loggerFactory,
			IResponseCacheService service
		)
		{
			_next = next;
			_options = options;
			_logger = loggerFactory.CreateLogger($"{typeof(ResponseCacheHttpMiddleware).Namespace}.{context.Identifier}.ResponseCache");
			_service = service;
		}

		/// <inheritdoc />
		public async Task<FluentHttpResponse> Invoke(FluentHttpMiddlewareContext context)
		{
			var request = context.Request;

			var options = request.GetResponseCachingOptions(_options);

			if (options.ShouldIgnore || options.Matcher != null && !options.Matcher(request))
				return await _next(context);
			var hash = request.GetHash();
			FluentHttpResponse response = null;

			if (!options.IsWriteOnly)
				response = await _service.Get(hash);
			if (response != null)
			{
				_logger.LogInformation("Pre-request - Returning a cached response {hash}", hash);
				return response;
			}

			response = await _next(context);

			_logger.LogInformation("Post-Response - Caching request... {hash}", hash);
			await _service.Set(hash, response);

			return response;
		}
	}
}

namespace FluentlyHttpClient
{
	/// <summary>
	/// Response Caching HTTP middleware extensions.
	/// </summary>
	public static class ResponseCacheHttpMiddlewareExtensions
	{
		private const string OptionsKey = "RESPONSE_CACHE_OPTIONS";

		#region Request Extensions

		/// <summary>
		/// Set response caching options for the request.
		/// </summary>
		/// <param name="requestBuilder">Request builder instance.</param>
		/// <param name="options">Options to set.</param>
		public static FluentHttpRequestBuilder WithResponseCachingOptions(this FluentHttpRequestBuilder requestBuilder, ResponseCacheHttpMiddlewareOptions options)
		{
			requestBuilder.Items[OptionsKey] = options;
			return requestBuilder;
		}

		/// <summary>
		/// Set response caching options for the request.
		/// </summary>
		/// <param name="requestBuilder">Request builder instance.</param>
		/// <param name="configure">Action to configure options.</param>
		public static FluentHttpRequestBuilder WithResponseCachingOptions(this FluentHttpRequestBuilder requestBuilder, Action<ResponseCacheHttpMiddlewareOptions> configure)
		{
			var options = new ResponseCacheHttpMiddlewareOptions();
			configure?.Invoke(options);
			return requestBuilder.WithResponseCachingOptions(options);
		}

		/// <summary>
		/// Get response caching options for the request.
		/// </summary>
		/// <param name="request">Request to get options from.</param>
		/// <param name="defaultOptions"></param>
		/// <returns>Returns merged logging options.</returns>
		public static ResponseCacheHttpMiddlewareOptions GetResponseCachingOptions(this FluentHttpRequest request, ResponseCacheHttpMiddlewareOptions defaultOptions = null)
		{
			if (!request.Items.TryGetValue(OptionsKey, out var result)) return defaultOptions;
			var options = (ResponseCacheHttpMiddlewareOptions)result;
			if (defaultOptions == null)
				return options;
			options.Matcher = defaultOptions.Matcher;
			options.IsWriteOnly = defaultOptions.IsWriteOnly;
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