using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using FluentlyHttpClient.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Xunit;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace FluentlyHttpClient.Middleware
{
	public interface IRequestCacheService
	{
		Task<FluentHttpResponse> Get(string hash, FluentHttpRequest request);
		Task Set(string hash, FluentHttpResponse response);
		bool Matcher(FluentHttpRequest request);
		string GenerateHash(FluentHttpRequest request);
	}

	public class MemoryRequestCacheService : IRequestCacheService
	{
		private readonly Dictionary<string, FluentHttpResponse> _cache = new Dictionary<string, FluentHttpResponse>();

		public async Task<FluentHttpResponse> Get(string hash, FluentHttpRequest request)
		{
			_cache.TryGetValue(hash, out var response);
			if (response == null)
				return null;

			var cloned = await Clone(response);
			return cloned;
		}

		public async Task Set(string hash, FluentHttpResponse response)
		{
			var cloned = await Clone(response);

			_cache[hash] = cloned;
		}

		// todo: make configurable instead?
		public bool Matcher(FluentHttpRequest request)
		{
			return true;
		}

		public string GenerateHash(FluentHttpRequest request)
		{
			var headers = HeadersToDictionary(request.Builder.DefaultHeaders);
			foreach (var requestHeader in request.Headers)
				headers[requestHeader.Key] = string.Join(";", requestHeader.Value);

			var urlHash = request.Uri.IsAbsoluteUri ? request.Uri : new Uri($"{request.Builder.BaseUrl.TrimEnd('/')}/{request.Uri}");
			var headersHash = HeadersToString(headers);

			var hash = $"method={request.Method};url={urlHash};headers={headersHash}";
			return hash;
		}

		// todo: move to be reusable
		private async Task<FluentHttpResponse> Clone(FluentHttpResponse response)
		{
			var contentString = await response.Content.ReadAsStringAsync();
			var contentType = response.Content.Headers.ContentType;
			var encoding = Encoding.GetEncoding(contentType.CharSet);

			var cloned = new FluentHttpResponse(new HttpResponseMessage(response.StatusCode)
			{
				Content = new StringContent(contentString, encoding, contentType.MediaType),
				ReasonPhrase = response.ReasonPhrase,
				StatusCode = response.StatusCode,
				Version = response.Message.Version,
				RequestMessage = response.Message.RequestMessage
			}, response.Items);

			CopyHeaders(cloned.Headers, response.Headers);

			return cloned;
		}

		// todo: change to extension method and make reusable CopyFrom(target)
		private static void CopyHeaders(HttpHeaders destination, HttpHeaders source)
		{
			foreach (var header in source)
				destination.TryAddWithoutValidation(header.Key, header.Value);
		}

		// todo: make reusable
		private static Dictionary<string, string> HeadersToDictionary(HttpHeaders headers)
			=> headers.ToDictionary(x => x.Key, x => string.Join(";", x.Value));

		// todo: make reusable
		private static string HeadersToString(Dictionary<string, string> headers)
		{
			var headersHash = "";
			foreach (var (key, value) in headers)
				headersHash += $"{key}={value}&";
			headersHash = headersHash.TrimEnd('&');
			return headersHash;
		}

	}

	/// <summary>
	/// Request Caching HTTP middleware options.
	/// </summary>
	public class RequestCacheHttpMiddlewareOptions
	{

	}

	/// <summary>
	/// Request caching middleware for HTTP client.
	/// </summary>
	public class RequestCacheHttpMiddleware : IFluentHttpMiddleware
	{
		private readonly FluentHttpRequestDelegate _next;
		private readonly RequestCacheHttpMiddlewareOptions _options;
		private readonly ILogger _logger;
		private readonly IRequestCacheService _requestCache;

		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		public RequestCacheHttpMiddleware(
			FluentHttpRequestDelegate next,
			RequestCacheHttpMiddlewareOptions options,
			ILogger<RequestCacheHttpMiddleware> logger,
			IRequestCacheService requestCache
		)
		{
			_next = next;
			_options = options;
			_logger = logger;
			_requestCache = requestCache;
		}

		/// <inheritdoc />
		public async Task<FluentHttpResponse> Invoke(FluentHttpRequest request)
		{
			if (!_requestCache.Matcher(request))
				return await _next(request);

			var hash = _requestCache.GenerateHash(request);

			var response = await _requestCache.Get(hash, request);

			if (response != null)
			{
				_logger.LogInformation("Pre-request - Returning a cached response {hash}", hash);
				return response;
			}

			response = await _next(request);

			_logger.LogInformation("Post-Response - Caching request... {hash}", hash);
			await _requestCache.Set(hash, response);

			return response;
		}
	}
}

namespace FluentlyHttpClient
{
	/// <summary>
	/// Request Caching HTTP middleware extensions.
	/// </summary>
	public static class RequestCacheHttpMiddlewareExtensions
	{
		/// <summary>
		/// Use logger middleware which logs out going requests and incoming responses.
		/// </summary>
		/// <param name="builder">Builder instance</param>
		/// <param name="options"></param>
		public static FluentHttpClientBuilder UseRequestCaching(this FluentHttpClientBuilder builder, RequestCacheHttpMiddlewareOptions options = null)
			=> builder.UseMiddleware<RequestCacheHttpMiddleware>(options ?? new RequestCacheHttpMiddlewareOptions());

		/// <summary>
		/// Use logger middleware which logs out going requests and incoming responses.
		/// </summary>
		/// <param name="builder">Builder instance</param>
		/// <param name="configure">Action to configure logging options.</param>
		public static FluentHttpClientBuilder UseRequestCaching(this FluentHttpClientBuilder builder, Action<RequestCacheHttpMiddlewareOptions> configure)
		{
			var options = new RequestCacheHttpMiddlewareOptions();
			configure?.Invoke(options);
			return builder.UseRequestCaching(options);
		}
	}
}

namespace FluentlyHttpClient.Test.Integration
{
	public class RequestCacheIntegrationTest
	{
		private static void ConfigureContainer(IServiceCollection container)
		{
			Log.Logger = new LoggerConfiguration()
				.WriteTo.Console()
				.WriteTo.Debug()
				.CreateLogger();
			container.AddSingleton<IRequestCacheService, MemoryRequestCacheService>()
				.AddLogging(x => x.AddSerilog())
				;
		}

		[Fact]
		public async Task ShouldMakeRequest_Get()
		{
			var fluentHttpClientFactory = ServiceTestUtil.GetNewClientFactory(ConfigureContainer);
			var clientBuilder = fluentHttpClientFactory.CreateBuilder("sketch7")
				//.WithBaseUrl("https://localhost:5001")
				.WithBaseUrl("http://local.sketch7.io:5000")
				.WithHeader("locale", "en-GB")
				.WithHeader("X-SSV-VERSION", "2019.02-2")
				.UseRequestCaching()
				.UseTimer()
			;
			var httpClient = fluentHttpClientFactory.Add(clientBuilder);
			var response = await httpClient.CreateRequest("/api/heroes/azmodan")
				.WithBearerAuthentication("XXX")
				.ReturnAsResponse<Hero>();

			var responseReason = response.ReasonPhrase;

			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			Assert.Equal("azmodan", response.Data.Key);

			response = await httpClient.CreateRequest("/api/heroes/azmodan")
				.ReturnAsResponse<Hero>();

			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			Assert.Equal("azmodan", response.Data.Key);

			response = await httpClient.CreateRequest("/api/heroes/azmodan")
				.ReturnAsResponse<Hero>();

			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			Assert.Equal("azmodan", response.Data.Key);
			Assert.Equal("Azmodan", response.Data.Name);
			Assert.Equal("Lord of Sins", response.Data.Title);
			Assert.Equal("Kestrel", response.Headers.Server.ToString());
			Assert.Equal(responseReason, response.ReasonPhrase);

			//Assert.Equal(HttpStatusCode.OK, response.Headers.);
		}
		// [Fact]
		//public async Task ShouldMakeRequest_Post()
		//{
		//	var fluentHttpClientFactory = ServiceTestUtil.GetNewClientFactory();
		//	var clientBuilder = fluentHttpClientFactory.CreateBuilder("sketch7")
		//		.WithBaseUrl("http://localhost:5001")
		//		.ConfigureFormatters(opts =>
		//			{
		//				opts.Default = _messagePackMediaTypeFormatter;
		//			})
		//		;
		//	var httpClient = fluentHttpClientFactory.Add(clientBuilder);
		//	var response = await httpClient.CreateRequest("/api/heroes")
		//		.AsPost()
		//		.WithBody(new Hero
		//		{
		//			Key = "valeera",
		//			Name = "Valeera",
		//			Title = "Shadow of the Ucrowned"
		//		})
		//		.ReturnAsResponse<Hero>();
		//	Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		//	Assert.Equal("valeera", response.Data.Key);
		//	Assert.Equal("Valeera", response.Data.Name);
		//	Assert.Equal("Shadow of the Ucrowned", response.Data.Title);
		//}
	}
}
