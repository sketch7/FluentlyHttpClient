using System.Diagnostics;
using System.Threading.Tasks;
using FluentlyHttpClient;
using Microsoft.Extensions.Logging;

namespace FluentlyHttpClient
{
	/// <summary>
	/// Timer middleware for Http client.
	/// </summary>
	public class TimerHttpMiddleware : IFluentHttpMiddleware
	{
		private readonly FluentHttpRequestDelegate _next;
		private readonly ILogger _logger;

		public TimerHttpMiddleware(FluentHttpRequestDelegate next, ILogger<TimerHttpMiddleware> logger)
		{
			_next = next;
			_logger = logger;
		}

		public async Task<FluentHttpResponse> Invoke(FluentHttpRequest request)
		{
			var watch = Stopwatch.StartNew();
			var response = await _next(request);
			var elapsed = watch.Elapsed;

			// todo: make configurable
			const int thresholdMillis = 250;
			if (_logger.IsEnabled(LogLevel.Warning) && elapsed.TotalMilliseconds >= thresholdMillis)
				LoggerExtensions.LogWarning(_logger, "Executed request {request} in {timeTakenMillis}ms", request, elapsed.TotalMilliseconds);
			else if (_logger.IsEnabled(LogLevel.Information))
				LoggerExtensions.LogInformation(_logger, "Executed request {request} in {timeTakenMillis}ms", request, elapsed.TotalMilliseconds);

			FluentResponseExtensions.SetTimeTaken(response, elapsed);
			return response;
		}
	}
}