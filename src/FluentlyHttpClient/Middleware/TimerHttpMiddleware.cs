using FluentlyHttpClient.Middleware;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace FluentlyHttpClient.Middleware
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
				_logger.LogWarning("Executed request {request} in {timeTakenMillis}ms", request, elapsed.TotalMilliseconds);
			else if (_logger.IsEnabled(LogLevel.Information))
				_logger.LogInformation("Executed request {request} in {timeTakenMillis}ms", request, elapsed.TotalMilliseconds);

			response.SetTimeTaken(elapsed);
			return response;
		}
	}
}

namespace FluentlyHttpClient
{
	public static class TimerFluentResponseExtensions
	{
		private const string TimeTakenKey = "TIME_TAKEN";

		/// <summary>
		/// Set time taken.
		/// </summary>
		/// <param name="response">Response instance.</param>
		/// <param name="value">Timespan value.</param>
		public static void SetTimeTaken(this FluentHttpResponse response, TimeSpan value)
		{
			response.Items.Add(TimeTakenKey, value);
		}

		/// <summary>
		/// Get time taken for the response. This is generally set via <see cref="TimerHttpMiddleware"/>.
		/// </summary>
		/// <param name="response">Response to get time from.</param>
		/// <returns>Returns timespan for the time taken.</returns>
		public static TimeSpan GetTimeTaken(this FluentHttpResponse response)
		{
			return (TimeSpan)response.Items[TimeTakenKey];
		}
	}
}