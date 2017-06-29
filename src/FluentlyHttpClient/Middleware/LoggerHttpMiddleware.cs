using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace FluentlyHttpClient.Middleware
{
	/// <summary>
	/// Logging middleware for HTTP client.
	/// </summary>
	public class LoggerHttpMiddleware : IFluentHttpMiddleware
	{
		private readonly FluentHttpRequestDelegate _next;
		private readonly ILogger _logger;

		public LoggerHttpMiddleware(FluentHttpRequestDelegate next, ILogger<LoggerHttpMiddleware> logger)
		{
			_next = next;
			_logger = logger;
		}

		public async Task<FluentHttpResponse> Invoke(FluentHttpRequest request)
		{
			if (_logger.IsEnabled(LogLevel.Information))
				_logger.LogInformation("Pre-request... {request}", request);

			var response = await _next(request);

			if (_logger.IsEnabled(LogLevel.Information))
				_logger.LogInformation("Post-request... {response}", response);
			return response;
		}
	}
}