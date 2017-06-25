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
	public class FluentHttpClientOptions
	{
		public string BaseUrl { get; set; }
		public TimeSpan Timeout { get; set; }
		public string Identifier { get; set; }
		public Dictionary<string, string> Headers { get; set; }
		public List<Type> Middleware { get; set; }
	}

	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class FluentHttpClient
	{
		private string DebuggerDisplay => $"[{Identifier}] BaseUrl: '{BaseUrl}', MiddlewareCount: {_middleware.Count}";

		/// <summary>
		/// Get the identifier (key) for this instance, which is registered with, within the factory.
		/// </summary>
		public string Identifier { get; }

		public string BaseUrl { get; }

		/// <summary>
		/// Raw http client. This should be avoided from being used.
		/// However if something is not exposed and its really needed, it can be used from here.
		/// </summary>
		public HttpClient RawHttpClient { get; }

		public MediaTypeFormatterCollection Formatters { get; } = new MediaTypeFormatterCollection();
		public HttpRequestHeaders Headers { get; }

		private readonly IServiceProvider _serviceProvider;
		private readonly IFluentHttpMiddlewareRunner _middlewareRunner;
		private readonly IList<Type> _middleware;

		public FluentHttpClient(FluentHttpClientOptions options, IServiceProvider serviceProvider,
			IFluentHttpMiddlewareRunner middlewareRunner)
		{
			_serviceProvider = serviceProvider;
			_middlewareRunner = middlewareRunner;
			RawHttpClient = Configure(options);
			Headers = RawHttpClient.DefaultRequestHeaders;
			_middleware = options.Middleware;
			Identifier = options.Identifier;
			BaseUrl = options.BaseUrl;
		}

		public Task<T> Get<T>(string url)
		{
			return CreateRequest(url)
				.AsGet()
				.Return<T>();
		}

		public Task<T> Post<T>(string url, object data, MediaTypeHeaderValue contentType = null)
		{
			return CreateRequest(url)
				.AsPost()
				.WithBody(data, contentType)
				.Return<T>();
		}

		public Task<T> Patch<T>(string url, object data, MediaTypeHeaderValue contentType = null)
		{
			return CreateRequest(url)
				.AsPatch()
				.WithBody(data, contentType)
				.Return<T>();
		}

		/// <summary>Get the formatter for an HTTP content type.</summary>
		/// <param name="contentType">The HTTP content type (or <c>null</c> to automatically select one).</param>
		/// <exception cref="System.InvalidOperationException">No MediaTypeFormatters are available on the API client for this content type.</exception>
		public MediaTypeFormatter GetFormatter(MediaTypeHeaderValue contentType = null)
		{
			if (!Formatters.Any())
				throw new InvalidOperationException("No media type formatters available.");

			MediaTypeFormatter formatter = contentType != null
				? Formatters.FirstOrDefault(x => x.SupportedMediaTypes.Any(m => m.MediaType == contentType.MediaType))
				: Formatters.FirstOrDefault();
			if (formatter == null)
				throw new InvalidOperationException($"No media type formatters are available for '{contentType}' content-type.");

			return formatter;
		}

		public FluentHttpRequestBuilder CreateRequest(string uriTemplate = null, object interpolationData = null)
		{
			var builder = ActivatorUtilities.CreateInstance<FluentHttpRequestBuilder>(_serviceProvider, this);
			return uriTemplate != null
				? builder.WithUri(uriTemplate, interpolationData)
				: builder;
		}

		private HttpClient Configure(FluentHttpClientOptions options)
		{
			var httpClient = new HttpClient
			{
				BaseAddress = new Uri(options.BaseUrl)
			};
			httpClient.DefaultRequestHeaders.Add("Accept", Formatters.SelectMany(x => x.SupportedMediaTypes).Select(x => x.MediaType));
			httpClient.Timeout = options.Timeout;

			foreach (var headerEntry in options.Headers)
				httpClient.DefaultRequestHeaders.Add(headerEntry.Key, headerEntry.Value);

			return httpClient;
		}

		public Task<FluentHttpResponse<T>> Send<T>(FluentHttpRequestBuilder builder) => Send<T>(builder.Build());

		public async Task<FluentHttpResponse<T>> Send<T>(FluentHttpRequest fluentRequest)
		{
			if (fluentRequest == null) throw new ArgumentNullException(nameof(fluentRequest));
			var response = await _middlewareRunner.Run<T>(_middleware, fluentRequest, async request =>
			{
				var result = await RawHttpClient.SendAsync(request.RawRequest);
				return ToFluentResponse<T>(result);
			});

			// todo: implement this better
			response.EnsureSuccessStatusCode();

			return (FluentHttpResponse<T>)response;
		}

		private static FluentHttpResponse<T> ToFluentResponse<T>(HttpResponseMessage response) =>
			new FluentHttpResponse<T>(response);

	}
}