using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentlyHttpClient.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog;
using Xunit;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace FluentlyHttpClient.Middleware
{

	public interface IRequestCacheService
	{
		Task<FluentHttpResponse> Get(FluentHttpRequest request);
		Task Set(FluentHttpRequest request, FluentHttpResponse response);
		bool Matcher(FluentHttpRequest request);
	}

	public class MemoryRequestCacheService : IRequestCacheService
	{
		private readonly Dictionary<string, FluentHttpResponse> _cache = new Dictionary<string, FluentHttpResponse>();

		public Task<FluentHttpResponse> Get(FluentHttpRequest request)
		{
			var hash = GenerateHash(request);

			_cache.TryGetValue(hash, out var response);
			return Task.FromResult(response);
		}

		public async Task Set(FluentHttpRequest request, FluentHttpResponse response)
		{
			var hash = GenerateHash(request);


			//var contetn = await response.Content.ReadAsStringAsync();
			var contentStream = await response.Content.ReadAsStreamAsync();
			//var x = await  response.Content.ReadAsHttpResponseMessageAsync();
			var c = JsonConvert.SerializeObject(response.Content);
			//var q = JsonConvert.DeserializeObject<FluentHttpResponse>(c);
			//response.Content.ReadAsAsync()
			var q = new FluentHttpResponse(new HttpResponseMessage(response.StatusCode)
			{
				//Content = new StringContent(contetn),
				Content = new StreamContent(contentStream),
				//Content = new ObjectContent(response.Me),
				//Headers = response.Headers,
				ReasonPhrase = response.ReasonPhrase,
				StatusCode = response.StatusCode,
				Version = response.Message.Version,
				RequestMessage = request.Message,
			});

			//response.
			_cache[hash] = q;
			//_cache[hash] = response;
			//return Task.CompletedTask;
		}

		public bool Matcher(FluentHttpRequest request)
		{
			return true;
		}

		// add Matcher

		private static string GenerateHash(FluentHttpRequest request)
		{
			// todo: use also base uri, however in PreRequest currently we dont get that
			string urlPart = request.Uri.IsAbsoluteUri ? request.Uri.PathAndQuery : request.Uri.ToString();
			var hash = $"[{request.Method}]{urlPart}";
			return hash;
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

			var response = await _requestCache.Get(request);

			if (response != null)
			{
				_logger.LogInformation("Pre-request - Returning a cached response");
				return response;
			}

			response = await _next(request);

			_logger.LogInformation("Post-Response - Caching request...");
			await _requestCache.Set(request, response);

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
				.UseRequestCaching()
					.UseTimer()
			;
			var httpClient = fluentHttpClientFactory.Add(clientBuilder);
			var response = await httpClient.CreateRequest("/api/heroes/azmodan")
				.ReturnAsResponse<Hero>();

			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			Assert.Equal("azmodan", response.Data.Key);

			response = await httpClient.CreateRequest("/api/heroes/azmodan")
				.ReturnAsResponse<Hero>();

			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			Assert.Equal("azmodan", response.Data.Key);
			Assert.Equal("Azmodan", response.Data.Name);
			Assert.Equal("Lord of Sins", response.Data.Title);
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
