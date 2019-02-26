using System.Threading.Tasks;
using FluentlyHttpClient.Caching;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace FluentlyHttpClient.Middleware
{
	/// <summary>
	/// Request Caching HTTP middleware options.
	/// </summary>
	public class ResponseCacheHttpMiddlewareOptions
	{

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
			if (!_requestCache.Matcher(request))
				return await _next(request);

			var hash = _requestCache.GenerateHash(request);

			var response = await _requestCache.Get(hash, request);

			if (response != null)
			{
				_logger.LogInformation("Pre-request - Returning a cached response {hash}", hash);
				return response;
			}

			response = await _next(request);

			_logger.LogInformation("Post-Response - Caching request... {hash}", hash);
			await _requestCache.Set(hash, response);

			// todo: set response.Item hash

			return response;
		}
	}
}