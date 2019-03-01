using System;
using System.Collections.Generic;
using System.Net.Http;
using FluentlyHttpClient.Middleware;
using Microsoft.Extensions.DependencyInjection;

namespace FluentlyHttpClient
{
	/// <summary>
	/// Class to configure <see cref="IFluentHttpClient"/> with a fluent API.
	/// </summary>
	public class FluentHttpClientBuilder : IFluentHttpHeaderBuilder<FluentHttpClientBuilder>
	{
		/// <summary>
		/// Gets the identifier specified.
		/// </summary>
		public string Identifier { get; private set; }

		private readonly IServiceProvider _serviceProvider;
		private readonly IFluentHttpClientFactory _fluentHttpClientFactory;
		private string _baseUrl;
		private TimeSpan _timeout;
		private readonly Dictionary<string, string> _headers = new Dictionary<string, string>();
		private readonly List<MiddlewareConfig> _middleware = new List<MiddlewareConfig>();
		private Action<FluentHttpRequestBuilder> _requestBuilderDefaults;
		private HttpMessageHandler _httpMessageHandler;
		private readonly FormatterOptions _formatterOptions = new FormatterOptions();

		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		public FluentHttpClientBuilder(
			IServiceProvider serviceProvider,
			IFluentHttpClientFactory fluentHttpClientFactory
		)
		{
			_serviceProvider = serviceProvider;
			_fluentHttpClientFactory = fluentHttpClientFactory;
		}

		/// <summary>
		/// Set base url for each request.
		/// </summary>
		/// <param name="url">Base url address to set. e.g. "https://api.sketch7.com"</param>
		/// <returns>Returns client builder for chaining.</returns>
		public FluentHttpClientBuilder WithBaseUrl(string url)
		{
			_baseUrl = url;
			return this;
		}

		/// <summary>
		/// Sets the timespan to wait before the request times out (in seconds).
		/// </summary>
		/// <param name="timeout">Timeout to set (in seconds).</param>
		/// <returns>Returns client builder for chaining.</returns>
		public FluentHttpClientBuilder WithTimeout(int timeout) => WithTimeout(TimeSpan.FromSeconds(timeout));

		/// <summary>
		/// Sets the timespan to wait before the request times out (in seconds).
		/// </summary>
		/// <param name="timeout">Timeout to set.</param>
		/// <returns>Returns client builder for chaining.</returns>
		public FluentHttpClientBuilder WithTimeout(TimeSpan timeout)
		{
			_timeout = timeout;
			return this;
		}

		/// <inheritdoc />
		public FluentHttpClientBuilder WithHeader(string key, string value)
		{
			_headers[key] = value;
			return this;
		}

		/// <inheritdoc />
		public FluentHttpClientBuilder WithHeaders(IDictionary<string, string> headers)
		{
			foreach (var item in headers)
				WithHeader(item.Key, item.Value);
			return this;
		}

		/// <summary>
		/// Set the identifier (unique key) for the HTTP Client.
		/// </summary>
		/// <param name="identifier">Identifier to set.</param>
		/// <returns>Returns client builder for chaining.</returns>
		public FluentHttpClientBuilder WithIdentifier(string identifier)
		{
			Identifier = identifier;
			return this;
		}

		/// <summary>
		/// Add a handler which allows to customize the <see cref="FluentHttpRequestBuilder"/> on <see cref="IFluentHttpClient.CreateRequest"/>.
		/// In order to specify defaults as desired, or so.
		/// </summary>
		/// <param name="requestBuilderDefaults">Action which pass <see cref="FluentHttpRequestBuilder"/> for customization.</param>
		/// <returns>Returns client builder for chaining.</returns>
		public FluentHttpClientBuilder WithRequestBuilderDefaults(Action<FluentHttpRequestBuilder> requestBuilderDefaults)
		{
			_requestBuilderDefaults = requestBuilderDefaults;
			return this;
		}

		/// <summary>
		/// Set HTTP handler stack to use for sending requests.
		/// </summary>
		/// <param name="handler">HTTP handler to use.</param>
		/// <returns>Returns client builder for chaining.</returns>
		public FluentHttpClientBuilder WithMessageHandler(HttpMessageHandler handler)
		{
			_httpMessageHandler = handler;
			return this;
		}

		/// <summary>
		/// Configure formatters to be used for content negotiation, for "Accept" and body media formats. e.g. JSON, XML, etc...
		/// </summary>
		/// <param name="configure">Action to configure formatters.</param>
		/// <returns>Returns client builder for chaining.</returns>
		public FluentHttpClientBuilder ConfigureFormatters(Action<FormatterOptions> configure)
		{
			configure(_formatterOptions);
			return this;
		}

		/// <summary>
		/// Register middleware for the HTTP client, which each request pass-through. <c>NOTE order matters</c>.
		/// </summary>
		/// <typeparam name="T">Middleware type.</typeparam>
		/// <returns>Returns client builder for chaining.</returns>
		public FluentHttpClientBuilder UseMiddleware<T>(params object[] args) where T : IFluentHttpMiddleware
			=> UseMiddleware(typeof(T), args);

		/// <summary>
		/// Register middleware for the HTTP client, which each request pass-through. <c>NOTE order matters</c>.
		/// </summary>
		/// <returns>Returns client builder for chaining.</returns>
		public FluentHttpClientBuilder UseMiddleware(Type middleware, params object[] args)
		{
			_middleware.Add(new MiddlewareConfig(middleware, args));
			return this;
		}

		/// <summary>
		/// Build up HTTP client options.
		/// </summary>
		/// <returns>Returns HTTP client options.</returns>
		public FluentHttpClientOptions Build() // todo: rename to BuildOptions
		{
			_formatterOptions.Resort();
			var options = new FluentHttpClientOptions
			{
				Timeout = _timeout,
				BaseUrl = _baseUrl,
				Identifier = Identifier,
				Headers = _headers,
				Middleware = _middleware,
				RequestBuilderDefaults = _requestBuilderDefaults,
				HttpMessageHandler = _httpMessageHandler,
				Formatters = _formatterOptions.Formatters,
				DefaultFormatter = _formatterOptions.Default
			};
			return options;
		}

		/// <summary>
		/// Builds a new http client (with default FluentHttpClient implementation).
		/// </summary>
		/// <param name="options"></param>
		/// <returns></returns>
		public IFluentHttpClient BuildClient(FluentHttpClientOptions options = null)
			=> BuildClient<FluentHttpClient>();// todo: rename to Build

		/// <summary>
		/// Build a new http client.
		/// </summary>
		/// <typeparam name="THttpClient">HttpClient type</typeparam>
		/// <param name="options"></param>
		/// <returns></returns>
		public IFluentHttpClient BuildClient<THttpClient>(FluentHttpClientOptions options = null) // todo: rename to Build
			where THttpClient : IFluentHttpClient
		{
			if (options == null)
				options = Build();

			if (string.IsNullOrEmpty(options.Identifier))
				throw ClientBuilderValidationException.FieldNotSpecified(nameof(options.Identifier));

			if (string.IsNullOrEmpty(options.BaseUrl))
				throw ClientBuilderValidationException.FieldNotSpecified(nameof(options.BaseUrl));

			var client = ActivatorUtilities.CreateInstance<THttpClient>(_serviceProvider, options, _fluentHttpClientFactory);
			return client;
		}

		/// <summary>
		/// Register to <see cref="IFluentHttpClientFactory"/>, same as <see cref="IFluentHttpClientFactory.Add(FluentHttpClientBuilder)"/> for convience.
		/// </summary>
		public FluentHttpClientBuilder Register()
		{
			_fluentHttpClientFactory.Add(this);
			return this;
		}

		/// <summary>
		/// Set options from the specified options config.
		/// </summary>
		/// <param name="options"></param>
		/// <returns></returns>
		public FluentHttpClientBuilder FromOptions(FluentHttpClientOptions options)
		{
			_timeout = options.Timeout;
			_baseUrl = options.BaseUrl;
			Identifier = options.Identifier;
			WithHeaders(options.Headers);
			_middleware.AddRange(options.Middleware);
			_requestBuilderDefaults = options.RequestBuilderDefaults;
			_httpMessageHandler = options.HttpMessageHandler;
			_formatterOptions.Formatters.AddRange(options.Formatters);
			_formatterOptions.Default = options.DefaultFormatter;

			return this;
		}
	}
}