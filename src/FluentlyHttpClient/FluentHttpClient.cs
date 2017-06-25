﻿using Microsoft.Extensions.DependencyInjection;
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

		public FluentHttpClient(FluentHttpClientOptions options, IServiceProvider serviceProvider, IFluentHttpMiddlewareRunner middlewareRunner)
		{
			_serviceProvider = serviceProvider;
			_middlewareRunner = middlewareRunner;
			RawHttpClient = Configure(options);
			Headers = RawHttpClient.DefaultRequestHeaders;
			_middleware = options.Middleware;
			Identifier = options.Identifier;
			BaseUrl = options.BaseUrl;
		}

		/// <summary>
		/// Create and send a HTTP GET request and return specified <see cref="T"/> as result.
		/// </summary>
		/// <typeparam name="T">Type to deserialize content to.</typeparam>
		/// <param name="uri">Request resource uri to send.</param>
		/// <returns>Returns task with the result data.</returns>
		public Task<T> Get<T>(string uri) => CreateRequest(uri)
			.AsGet()
			.WithSuccessStatus()
			.Return<T>();

		/// <summary>
		/// Create and send a HTTP POST request and return specified <see cref="T"/> as result.
		/// </summary>
		/// <typeparam name="T">Type to deserialize content to.</typeparam>
		/// <param name="uri">Request resource uri to send.</param>
		/// <param name="data">Payload data content to send.</param>
		/// <param name="contentType">(Optional) content type to use when sending data.</param>
		/// <returns>Returns task with the result data.</returns>
		public Task<T> Post<T>(string uri, object data, MediaTypeHeaderValue contentType = null) => CreateRequest(uri)
			.AsPost()
			.WithSuccessStatus()
			.WithBody(data, contentType)
			.Return<T>();

		/// <summary>
		/// Create and send a HTTP PUT request and return specified <see cref="T"/> as result.
		/// </summary>
		/// <typeparam name="T">Type to deserialize content to.</typeparam>
		/// <param name="uri">Request resource uri to send.</param>
		/// <param name="data">Payload data content to send.</param>
		/// <param name="contentType">(Optional) content type to use when sending data.</param>
		/// <returns>Returns task with the result data.</returns>
		public Task<T> Put<T>(string uri, object data, MediaTypeHeaderValue contentType = null) => CreateRequest(uri)
			.AsPut()
			.WithSuccessStatus()
			.WithBody(data, contentType)
			.Return<T>();

		/// <summary>
		/// Create and send a HTTP PATCH request and return specified <see cref="T"/> as result.
		/// </summary>
		/// <typeparam name="T">Type to deserialize content to.</typeparam>
		/// <param name="uri">Request resource uri to send.</param>
		/// <param name="data">Payload data content to send.</param>
		/// <param name="contentType">(Optional) content type to use when sending data.</param>
		/// <returns>Returns task with the result data.</returns>
		public Task<T> Patch<T>(string uri, object data, MediaTypeHeaderValue contentType = null) => CreateRequest(uri)
			.AsPatch()
			.WithSuccessStatus()
			.WithBody(data, contentType)
			.Return<T>();

		/// <summary>
		/// Create and send a HTTP DELETE request and return specified <see cref="T"/> as result.
		/// </summary>
		/// <typeparam name="T">Type to deserialize content to.</typeparam>
		/// <param name="uri">Request resource uri to send.</param>
		/// <param name="data">Payload data content to send.</param>
		/// <param name="contentType">(Optional) content type to use when sending data.</param>
		/// <returns>Returns task with the result data.</returns>
		public Task<T> Delete<T>(string uri, object data, MediaTypeHeaderValue contentType = null) => CreateRequest(uri)
			.AsDelete()
			.WithSuccessStatus()
			.WithBody(data, contentType)
			.Return<T>();

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

		public Task<FluentHttpResponse> Send(FluentHttpRequestBuilder builder) => Send(builder.Build());

		public async Task<FluentHttpResponse> Send(FluentHttpRequest fluentRequest)
		{
			if (fluentRequest == null) throw new ArgumentNullException(nameof(fluentRequest));
			var response = await _middlewareRunner.Run(_middleware, fluentRequest, async request =>
			{
				var result = await RawHttpClient.SendAsync(request.RawRequest);
				return ToFluentResponse(result);
			});

			if (fluentRequest.HasSuccessStatusOrThrow)
				response.EnsureSuccessStatusCode();

			return response;
		}

		private static FluentHttpResponse ToFluentResponse(HttpResponseMessage response) =>
			new FluentHttpResponse(response);

	}
}