using FluentlyHttpClient;
using FluentlyHttpClient.Internal;
using FluentlyHttpClient.Middleware;
using Microsoft.Extensions.Logging;
using System.Net;

namespace FluentlyHttpClient.Middleware
{
	/// <summary>
	/// Logger HTTP middleware options.
	/// </summary>
	public record LoggerHttpMiddlewareOptions
	{
		/// <summary>
		/// Gets or sets whether the request log should be detailed e.g. include body. Note: This should only be enabled for development or as needed,
		/// as it will reduce performance.
		/// </summary>
		public bool? ShouldLogDetailedRequest { get; set; }

		/// <summary>
		/// Gets or sets whether the response log should be detailed e.g. include body. Note: This should only be enabled for development or as needed,
		/// as it will reduce performance.
		/// </summary>
		public bool? ShouldLogDetailedResponse { get; set; }

		/// <summary>
		/// Gets or sets whether the logs are consolidated in one for the request and response.
		/// </summary>
		public bool? IsCondensed { get; set; } = true;
	}

	/// <summary>
	/// Logging middleware for HTTP client.
	/// </summary>
	public class LoggerHttpMiddleware : IFluentHttpMiddleware
	{
		private readonly FluentHttpMiddlewareDelegate _next;
		private readonly LoggerHttpMiddlewareOptions _options;
		private readonly ILogger _logger;

		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		public LoggerHttpMiddleware(
			FluentHttpMiddlewareDelegate next,
			FluentHttpMiddlewareClientContext context,
			LoggerHttpMiddlewareOptions options,
			ILoggerFactory loggerFactory
		)
		{
			_next = next;
			_options = options;
			_logger = loggerFactory.CreateLogger($"{typeof(LoggerHttpMiddleware).Namespace}.{context.Identifier}.Logger");
		}

		/// <inheritdoc />
		public async Task<FluentHttpResponse> Invoke(FluentHttpMiddlewareContext context)
		{
			var request = context.Request;
			if (!_logger.IsEnabled(LogLevel.Information))
				return await _next(context);

			var options = request.GetLoggingOptions(_options) ?? _options;
			var watch = ValueStopwatch.StartNew();
			FluentHttpResponse response;
			if (options.IsCondensed.GetValueOrDefault(false)
				&& !options.ShouldLogDetailedRequest.GetValueOrDefault(false)
				&& !options.ShouldLogDetailedResponse.GetValueOrDefault(false))
			{
				response = await _next(context);
				_logger.LoggerHttp_CondensedRequest(request.Method, request.Uri!, response.StatusCode, watch.GetElapsedTime().TotalMilliseconds);
				return response;
			}

			if (!(options.ShouldLogDetailedRequest ?? false))
				_logger.LoggerHttp_Request(request);
			else
			{
				string? requestContent = null;
				if (request.Message.Content != null)
					requestContent = await request.Message.Content.ReadAsStringAsync();
				_logger.LoggerHttp_RequestDetailed(request, request.Headers.ToFormattedString(), requestContent);
			}

			response = await _next(context);
			var stopwatchElapsed = watch.GetElapsedTime();
			if (response.Content == null || !(options.ShouldLogDetailedResponse ?? false))
			{
				_logger.LoggerHttp_Response(response, stopwatchElapsed.TotalMilliseconds);
				return response;
			}

			var responseContent = await response.Content.ReadAsStringAsync();
			_logger.LoggerHttp_ResponseDetailed(response, response.Headers.ToFormattedString(), responseContent, stopwatchElapsed.TotalMilliseconds);
			return response;
		}
	}
}

namespace FluentlyHttpClient
{
	/// <summary>
	/// Logger HTTP middleware extensions.
	/// </summary>
	public static class LoggerHttpMiddlewareExtensions
	{
		private const string LoggingOptionsKey = "LOGGING_OPTIONS";

		#region Request Extensions
		/// <summary>
		/// Set logging options for the request.
		/// </summary>
		/// <param name="requestBuilder">Request builder instance.</param>
		/// <param name="options">Logging options to set.</param>
		public static FluentHttpRequestBuilder WithLoggingOptions(this FluentHttpRequestBuilder requestBuilder, LoggerHttpMiddlewareOptions options)
		{
			requestBuilder.Items[LoggingOptionsKey] = options;
			return requestBuilder;
		}

		/// <inheritdoc cref="WithLoggingOptions(FluentHttpRequestBuilder,LoggerHttpMiddlewareOptions)"/>
		public static FluentHttpRequestBuilder WithLoggingOptions(this FluentHttpRequestBuilder requestBuilder, Action<LoggerHttpMiddlewareOptions>? configure)
		{
			var options = new LoggerHttpMiddlewareOptions();
			configure?.Invoke(options);
			return requestBuilder.WithLoggingOptions(options);
		}

		/// <summary>
		/// Get logging option for the request.
		/// </summary>
		/// <param name="request">Request to get options from.</param>
		/// <param name="defaultOptions"></param>
		/// <returns>Returns merged logging options.</returns>
		public static LoggerHttpMiddlewareOptions? GetLoggingOptions(this FluentHttpRequest request, LoggerHttpMiddlewareOptions? defaultOptions = null)
		{
			if (!request.Items.TryGetValue(LoggingOptionsKey, out var result)) return defaultOptions;
			var options = (LoggerHttpMiddlewareOptions)result;
			if (defaultOptions == null)
				return options;
			options.ShouldLogDetailedRequest ??= defaultOptions.ShouldLogDetailedRequest;
			options.ShouldLogDetailedResponse ??= defaultOptions.ShouldLogDetailedResponse;
			return options;
		}
		#endregion

		/// <summary>
		/// Use logger middleware which logs out going requests and incoming responses.
		/// </summary>
		/// <param name="builder">Builder instance</param>
		/// <param name="options"></param>
		public static FluentHttpClientBuilder UseLogging(this FluentHttpClientBuilder builder, LoggerHttpMiddlewareOptions? options = null)
			=> builder.UseMiddleware<LoggerHttpMiddleware>(options ?? new LoggerHttpMiddlewareOptions());

		/// <inheritdoc cref="UseLogging(FluentHttpClientBuilder,LoggerHttpMiddlewareOptions?)"/>
		/// <param name="builder">Builder instance</param>
		/// <param name="configure">Action to configure logging options.</param>
		public static FluentHttpClientBuilder UseLogging(this FluentHttpClientBuilder builder, Action<LoggerHttpMiddlewareOptions>? configure)
		{
			var options = new LoggerHttpMiddlewareOptions();
			configure?.Invoke(options);
			return builder.UseLogging(options);
		}
	}
}

internal static partial class LogExtensions
{
	[LoggerMessage(LogLevel.Information, "HTTP request [{method}] {requestUrl} responded {statusCode:D} in {elapsed:n0}ms")]
	internal static partial void LoggerHttp_CondensedRequest(this ILogger logger, HttpMethod method, Uri requestUrl, HttpStatusCode statusCode, double elapsed);

	[LoggerMessage(LogLevel.Information, "Pre - request... {request}")]
	internal static partial void LoggerHttp_Request(this ILogger logger, FluentHttpRequest request);

	[LoggerMessage(LogLevel.Information, "Pre-request... {request}\nHeaders: {headers}\nContent: {requestContent}")]
	internal static partial void LoggerHttp_RequestDetailed(this ILogger logger, FluentHttpRequest request, string headers, string? requestContent);

	[LoggerMessage(LogLevel.Information, "Post-request... {response} in {elapsed:n0}ms")]
	internal static partial void LoggerHttp_Response(this ILogger logger, FluentHttpResponse response, double elapsed);

	[LoggerMessage(LogLevel.Information, "Post-request... {response}\nHeaders: {headers}\nContent: {responseContent} in {elapsed:n0}ms")]
	internal static partial void LoggerHttp_ResponseDetailed(this ILogger logger, FluentHttpResponse response, string headers, string? responseContent, double elapsed);
}