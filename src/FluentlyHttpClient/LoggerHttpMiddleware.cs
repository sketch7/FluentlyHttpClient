using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace FluentlyHttpClient
{
	/// <summary>
	/// Logging middleware for Http client.
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

	public static class FluentResponseExtensions
	{
		private const string TimeTakenKey = "TIME_TAKEN";

		public static void SetTimeTaken(this FluentHttpResponse response, TimeSpan value)
		{
			response.Items.Add(TimeTakenKey, value);
		}

		public static TimeSpan GetTimeTaken(this FluentHttpResponse response)
		{
			return (TimeSpan)response.Items[TimeTakenKey];
		}
	}
}
