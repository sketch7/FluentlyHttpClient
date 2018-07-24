using FluentlyHttpClient.Middleware;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace FluentlyHttpClient
{
	/// <summary>
	/// Interface for sending HTTP requests with a high level fluent API.
	/// </summary>
	public interface IFluentHttpClient : IDisposable
	{
		/// <summary>
		/// Get the identifier (key) for this instance, which is registered with, within the factory.
		/// </summary>
		string Identifier { get; }

		/// <summary>
		/// Gets the base uri address for each request.
		/// </summary>
		string BaseUrl { get; }

		/// <summary>
		/// Underlying HTTP client. This should be avoided from being used,
		/// however if something is not exposed and its really needed, it can be used from here.
		/// </summary>
		HttpClient RawHttpClient { get; }

		/// <summary>
		/// Formatters to be used for content negotiation for "Accept" and also sending formats. e.g. (JSON, XML)
		/// </summary>
		MediaTypeFormatterCollection Formatters { get; }

		/// <summary>
		/// Gets the headers which should be sent with each request.
		/// </summary>
		HttpRequestHeaders Headers { get; }

		/// <summary>
		/// Gets the default formatter to be used when serializing body content. e.g. JSON, XML, etc...
		/// </summary>
		MediaTypeFormatter DefaultFormatter { get; }

		/// <summary>Get the formatter for an HTTP content type.</summary>
		/// <param name="contentType">The HTTP content type (or <c>null</c> to automatically select one).</param>
		/// <exception cref="InvalidOperationException">No MediaTypeFormatters are available on the API client for this content type.</exception>
		MediaTypeFormatter GetFormatter(MediaTypeHeaderValue contentType = null);

		/// <summary>
		/// Create a new request builder which can be configured fluently.
		/// </summary>
		/// <param name="uriTemplate">Uri resource template e.g. <c>"/org/{id}"</c></param>
		/// <param name="interpolationData">Data to interpolate within the Uri template place holders e.g. <c>{id}</c>. Can be either dictionary or object.</param>
		/// <returns>Returns a new request builder.</returns>
		FluentHttpRequestBuilder CreateRequest(string uriTemplate = null, object interpolationData = null);

		/// <summary>
		/// Build and send HTTP request.
		/// </summary>
		/// <param name="builder">Request builder to build request from.</param>
		/// <returns>Returns HTTP response.</returns>
		Task<FluentHttpResponse> Send(FluentHttpRequestBuilder builder);

		/// <summary>
		/// Send HTTP request.
		/// </summary>
		/// <param name="fluentRequest">HTTP fluent request to send.</param>
		/// <returns>Returns HTTP response.</returns>
		Task<FluentHttpResponse> Send(FluentHttpRequest fluentRequest);
	}

	/// <summary>
	/// Provides a class for sending HTTP requests with a high level fluent API.
	/// </summary>
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class FluentHttpClient : IFluentHttpClient
	{
		private string DebuggerDisplay => $"[{Identifier}] BaseUrl: '{BaseUrl}', MiddlewareCount: {_middleware.Count}";

		/// <inheritdoc />
		public string Identifier { get; }

		/// <inheritdoc />
		public string BaseUrl { get; }

		/// <inheritdoc />
		public HttpClient RawHttpClient { get; }

		/// <inheritdoc />
		public MediaTypeFormatterCollection Formatters { get; }

		/// <inheritdoc />
		public MediaTypeFormatter DefaultFormatter { get; }

		/// <inheritdoc />
		public HttpRequestHeaders Headers { get; }

		private readonly Action<FluentHttpRequestBuilder> _requestBuilderDefaults;
		private readonly IServiceProvider _serviceProvider;
		private readonly IFluentHttpMiddlewareRunner _middlewareRunner;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly List<MiddlewareConfig> _middleware;

		/// <summary>
		/// Initializes an instance of <see cref="FluentHttpClient"/>.
		/// </summary>
		/// <param name="options"></param>
		/// <param name="serviceProvider"></param>
		/// <param name="middlewareRunner"></param>
		/// <param name="httpClientFactory"></param>
		public FluentHttpClient(
			FluentHttpClientOptions options,
			IServiceProvider serviceProvider,
			IFluentHttpMiddlewareRunner middlewareRunner,
			IHttpClientFactory httpClientFactory
		)
		{
			_serviceProvider = serviceProvider;
			_middlewareRunner = middlewareRunner;
			_httpClientFactory = httpClientFactory;
			_middleware = options.Middleware;
			_requestBuilderDefaults = options.RequestBuilderDefaults;

			Identifier = options.Identifier;
			BaseUrl = options.BaseUrl;
			Formatters = options.Formatters;
			DefaultFormatter = options.DefaultFormatter;

			RawHttpClient = Configure(options);
			Headers = RawHttpClient.DefaultRequestHeaders;
		}

		/// <inheritdoc />
		public MediaTypeFormatter GetFormatter(MediaTypeHeaderValue contentType = null)
		{
			if (!Formatters.Any())
				throw new InvalidOperationException("No media type formatters available.");

			MediaTypeFormatter formatter = contentType != null
				? Formatters.FirstOrDefault(x => x.SupportedMediaTypes.Any(m => m.MediaType == contentType.MediaType))
				: DefaultFormatter ?? Formatters.FirstOrDefault();
			if (formatter == null)
				throw new InvalidOperationException($"No media type formatters are available for '{contentType}' content-type.");

			return formatter;
		}

		/// <inheritdoc />
		public FluentHttpRequestBuilder CreateRequest(string uriTemplate = null, object interpolationData = null)
		{
			var builder = ActivatorUtilities.CreateInstance<FluentHttpRequestBuilder>(_serviceProvider, this);
			_requestBuilderDefaults?.Invoke(builder);
			return uriTemplate != null
				? builder.WithUri(uriTemplate, interpolationData)
				: builder;
		}

		/// <inheritdoc />
		public Task<FluentHttpResponse> Send(FluentHttpRequestBuilder builder) => Send(builder.Build());

		/// <inheritdoc />
		public async Task<FluentHttpResponse> Send(FluentHttpRequest fluentRequest)
		{
			if (fluentRequest == null) throw new ArgumentNullException(nameof(fluentRequest));
			var response = await _middlewareRunner.Run(_middleware, fluentRequest, async request =>
			{
				var result = await RawHttpClient.SendAsync(request.Message, request.CancellationToken)
					.ConfigureAwait(false);
				return ToFluentResponse(result, request.Items);
			}).ConfigureAwait(false);

			if (fluentRequest.HasSuccessStatusOrThrow)
				response.EnsureSuccessStatusCode();

			return response;
		}

		private HttpClient Configure(FluentHttpClientOptions options)
		{
			var httpClient = options.HttpMessageHandler == null
			 ? _httpClientFactory.CreateClient()
			 : new HttpClient(options.HttpMessageHandler);
			httpClient.BaseAddress = new Uri(options.BaseUrl);
			httpClient.DefaultRequestHeaders.Add(HeaderTypes.Accept, Formatters.SelectMany(x => x.SupportedMediaTypes).Select(x => x.MediaType));
			httpClient.Timeout = options.Timeout;

			foreach (var headerEntry in options.Headers)
				httpClient.DefaultRequestHeaders.Add(headerEntry.Key, headerEntry.Value);

			return httpClient;
		}

		/// <inheritdoc />
		public void Dispose()
		{
			RawHttpClient?.Dispose();
		}

		private static FluentHttpResponse ToFluentResponse(HttpResponseMessage response, IDictionary<object, object> items)
			=> new FluentHttpResponse(response, items);

	}
}