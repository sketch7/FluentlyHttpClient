using FluentlyHttpClient.Internal;
using FluentlyHttpClient.Middleware;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace FluentlyHttpClient.Middleware
{
	/// <summary>
	/// Timer HTTP middleware options.
	/// </summary>
	public class TimerHttpMiddlewareOptions
	{
		/// <summary>
		/// Gets or sets the threshold warning timespan in order to log as warning.
		/// </summary>
		public TimeSpan WarnThreshold { get; set; } = TimeSpan.FromMilliseconds(400);
	}

	/// <summary>
	/// Timer middleware for HTTP client.
	/// </summary>
	public class TimerHttpMiddleware : IFluentHttpMiddleware
	{
		private const string TimeTakenMessage = "Executed request {request} in {elapsed:n0}ms";
		private readonly FluentHttpMiddlewareDelegate _next;
		private readonly TimerHttpMiddlewareOptions _options;
		private readonly ILogger _logger;

		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		public TimerHttpMiddleware(
			FluentHttpMiddlewareDelegate next,
			FluentHttpMiddlewareClientContext context,
			TimerHttpMiddlewareOptions options,
			ILoggerFactory loggerFactory
		)
		{
			_next = next;
			_options = options;
			_logger = loggerFactory.CreateLogger($"{typeof(TimerHttpMiddleware).Namespace}.{context.Identifier}.Timer");

			if (_options.WarnThreshold <= TimeSpan.Zero)
				throw new ArgumentException($"{nameof(_options.WarnThreshold)} must be greater than Zero.");
		}

		/// <inheritdoc />
		public async Task<FluentHttpResponse> Invoke(FluentHttpMiddlewareContext context)
		{
			var request = context.Request;
			TimeSpan stopwatchElapsed = default;

			FluentHttpResponse response;
			try
			{
				var watch = ValueStopwatch.StartNew();
				response = await _next(context);
				stopwatchElapsed = watch.GetElapsedTime();
			}
			finally
			{
				var threshold = request.GetTimerWarnThreshold() ?? _options.WarnThreshold;
				if (_logger.IsEnabled(LogLevel.Warning) && stopwatchElapsed > threshold)
					_logger.LogWarning(TimeTakenMessage, request, stopwatchElapsed.TotalMilliseconds);
				else if (_logger.IsEnabled(LogLevel.Debug))
					_logger.LogDebug(TimeTakenMessage, request, stopwatchElapsed.TotalMilliseconds);
			}

			return response.SetTimeTaken(stopwatchElapsed);
		}
	}
}

namespace FluentlyHttpClient
{
	/// <summary>
	/// Timer HTTP middleware extensions.
	/// </summary>
	public static class TimerHttpMiddlewareExtensions
	{
		private const string TimeTakenKey = "TIMER_TIME_TAKEN";
		private const string WarnThresholdOptionKey = "TIMER_OPTION_WARN_THRESHOLD";

		#region Request Extensions
		/// <summary>
		/// Set timer warn threshold for request.
		/// </summary>
		/// <param name="requestBuilder">Request builder instance.</param>
		/// <param name="value">Timespan value.</param>
		public static FluentHttpRequestBuilder WithTimerWarnThreshold(this FluentHttpRequestBuilder requestBuilder, TimeSpan value)
		{
			requestBuilder.Items[WarnThresholdOptionKey] = value;
			return requestBuilder;
		}

		/// <summary>
		/// Get timer warn threshold option for the request.
		/// </summary>
		/// <param name="request">Request to get time from.</param>
		/// <returns>Returns timespan for the time taken.</returns>
		public static TimeSpan? GetTimerWarnThreshold(this FluentHttpRequest request)
		{
			if (request.Items.TryGetValue(WarnThresholdOptionKey, out var result))
				return (TimeSpan)result;
			return null;
		}
		#endregion

		#region Response Extensions
		/// <summary>
		/// Set time taken.
		/// </summary>
		/// <param name="response">Response instance.</param>
		/// <param name="value">Timespan value.</param>
		public static FluentHttpResponse SetTimeTaken(this FluentHttpResponse response, TimeSpan value)
		{
			response.Items.Add(TimeTakenKey, value);
			return response;
		}

		/// <summary>
		/// Get time taken for the response. This is generally set via <see cref="TimerHttpMiddleware"/>.
		/// </summary>
		/// <param name="response">Response to get time from.</param>
		/// <returns>Returns timespan for the time taken.</returns>
		public static TimeSpan GetTimeTaken(this FluentHttpResponse response)
			=> (TimeSpan)response.Items[TimeTakenKey];
		#endregion

		/// <summary>
		/// Use timer middleware which measures how long the request takes.
		/// </summary>
		/// <param name="builder">Builder instance</param>
		/// <param name="options">Options to specify for the timer options.</param>
		public static FluentHttpClientBuilder UseTimer(this FluentHttpClientBuilder builder, TimerHttpMiddlewareOptions? options = null)
			=> builder.UseMiddleware<TimerHttpMiddleware>(options ?? new TimerHttpMiddlewareOptions());

		/// <summary>
		/// Use timer middleware which measures how long the request takes.
		/// </summary>
		/// <param name="builder">Builder instance</param>
		/// <param name="configure">Action to configure timer options.</param>
		public static FluentHttpClientBuilder UseTimer(this FluentHttpClientBuilder builder, Action<TimerHttpMiddlewareOptions>? configure)
		{
			var options = new TimerHttpMiddlewareOptions();
			configure?.Invoke(options);
			return builder.UseTimer(options);
		}
	}
}