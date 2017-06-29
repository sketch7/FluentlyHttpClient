using FluentlyHttpClient.Middleware;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace FluentlyHttpClient.Middleware
{
	public class TimerHttpMiddlewareOptions
	{
		/// <summary>
		/// Gets or sets the threshold warning timespan in order to log as warning.
		/// </summary>
		public TimeSpan WarnThreshold { get; set; } = TimeSpan.FromMilliseconds(250);
	}
	
	/// <summary>
	/// Timer middleware for Http client.
	/// </summary>
	public class TimerHttpMiddleware : IFluentHttpMiddleware
	{
		private readonly FluentHttpRequestDelegate _next;
		private readonly TimerHttpMiddlewareOptions _options;
		private readonly ILogger _logger;

		public TimerHttpMiddleware(FluentHttpRequestDelegate next, TimerHttpMiddlewareOptions options, ILogger<TimerHttpMiddleware> logger)
		{
			_next = next;
			_options = options;
			_logger = logger;

			if(_options.WarnThreshold <= TimeSpan.Zero)
				throw new ArgumentException($"{nameof(_options.WarnThreshold)} must be greater than Zero.");
		}

		public async Task<FluentHttpResponse> Invoke(FluentHttpRequest request)
		{
			var watch = Stopwatch.StartNew();
			var response = await _next(request);
			var elapsed = watch.Elapsed;
			
			if (_logger.IsEnabled(LogLevel.Warning) && elapsed > _options.WarnThreshold)
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
			=> response.Items.Add(TimeTakenKey, value);

		/// <summary>
		/// Get time taken for the response. This is generally set via <see cref="TimerHttpMiddleware"/>.
		/// </summary>
		/// <param name="response">Response to get time from.</param>
		/// <returns>Returns timespan for the time taken.</returns>
		public static TimeSpan GetTimeTaken(this FluentHttpResponse response)
			=> (TimeSpan)response.Items[TimeTakenKey];
	}

	public static class FluentlyHttpMiddlwareExtensions
	{
		/// <summary>
		/// Use timer middleware which measures how long the request takes.
		/// </summary>
		/// <param name="builder">Builder instance</param>
		/// <param name="options">Options to specify for the timer middleware.</param>
		public static FluentHttpClientBuilder UseTimer(this FluentHttpClientBuilder builder, TimerHttpMiddlewareOptions options = null)
			=> builder.UseMiddleware<TimerHttpMiddleware>(options ?? new TimerHttpMiddlewareOptions());
		
	}
}