using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace FluentlyHttpClient
{
	public class LoggerHttpMiddleware : IFluentHttpMiddleware
	{
		private readonly FluentHttpRequestDelegate _next;
		private readonly ILogger _logger;

		public LoggerHttpMiddleware(FluentHttpRequestDelegate next, ILogger<LoggerHttpMiddleware> logger)
		{
			_next = next;
			_logger = logger;
		}

		public async Task<IFluentHttpResponse> Invoke(FluentHttpRequest request)
		{
			if (_logger.IsEnabled(LogLevel.Information))
				_logger.LogInformation("Pre-request... {request}", request);

			var response = await _next(request);

			if (_logger.IsEnabled(LogLevel.Information))
				_logger.LogInformation("Post-request... {response}", response);
			return response;
		}
	}
	public class TimerHttpMiddleware : IFluentHttpMiddleware
	{
		private readonly FluentHttpRequestDelegate _next;
		private readonly ILogger _logger;

		public TimerHttpMiddleware(FluentHttpRequestDelegate next, ILogger<TimerHttpMiddleware> logger)
		{
			_next = next;
			_logger = logger;
		}

		public async Task<IFluentHttpResponse> Invoke(FluentHttpRequest request)
		{
			var watch = Stopwatch.StartNew();
			var response = await _next(request);
			var elapsed = watch.Elapsed;

			// todo: make configurable
			const int thresholdMillis = 250;
			if (_logger.IsEnabled(LogLevel.Warning) && elapsed.TotalMilliseconds >= thresholdMillis)
				_logger.LogWarning("Executed request {request} in {timeTakenMillis}ms", request, elapsed.TotalMilliseconds);
			else if (_logger.IsEnabled(LogLevel.Information))
				_logger.LogInformation("Executed request {request} in {timeTakenMillis}ms", request, elapsed.TotalMilliseconds);

			response.SetTimeTaken(elapsed);
			return response;
		}
	}

	public static class FluentResponseExtensions
	{
		private const string TimeTakenKey = "TIME_TAKEN";

		public static void SetTimeTaken(this IFluentHttpResponse response, TimeSpan value)
		{
			response.Items.Add(TimeTakenKey, value);
		}

		public static TimeSpan GetTimeTaken(this IFluentHttpResponse response)
		{
			return (TimeSpan)response.Items[TimeTakenKey];
		}
	}
}
