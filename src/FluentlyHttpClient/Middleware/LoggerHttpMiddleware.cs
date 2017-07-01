using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using FluentlyHttpClient.Middleware;

namespace FluentlyHttpClient.Middleware
{
	/// <summary>
	/// Logging middleware for HTTP client.
	/// </summary>
	public class LoggerHttpMiddleware : IFluentHttpMiddleware
	{
		private readonly FluentHttpRequestDelegate _next;
		private readonly ILogger _logger;

		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		public LoggerHttpMiddleware(FluentHttpRequestDelegate next, ILogger<LoggerHttpMiddleware> logger)
		{
			_next = next;
			_logger = logger;
		}

		/// <summary>
		/// Function to invoke.
		/// </summary>
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

namespace FluentlyHttpClient
{
	/// <summary>
	/// Logger HTTP middleware extensions.
	/// </summary>
	public static class LoggerHttpMiddlwareExtensions
	{
		/// <summary>
		/// Use logger middleware which logs out going requests and incoming responses.
		/// </summary>
		/// <param name="builder">Builder instance</param>
		public static FluentHttpClientBuilder UseLogging(this FluentHttpClientBuilder builder)
			=> builder.UseMiddleware<LoggerHttpMiddleware>();
	}
}