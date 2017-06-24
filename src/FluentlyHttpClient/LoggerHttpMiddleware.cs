using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace FluentlyHttp
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
			_logger.LogInformation("Pre-request... [{method}] {url}", request.Method, request.Url);
			var response = await _next(request);
			_logger.LogInformation("Post-request... {status}", response.StatusCode);
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
			_logger.LogInformation("{timeTaken}", elapsed);
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
