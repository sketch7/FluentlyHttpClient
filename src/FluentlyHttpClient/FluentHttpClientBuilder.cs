using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using FluentlyHttpClient.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;

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
		private readonly FluentHttpMiddlewareBuilder _middlewareBuilder;
		private string _baseUrl;
		private TimeSpan _timeout;
		private readonly FluentHttpHeaders _headers = new FluentHttpHeaders();
		private Action<FluentHttpRequestBuilder> _requestBuilderDefaults;
		private HttpMessageHandler _httpMessageHandler;
		private readonly FormatterOptions _formatterOptions = new FormatterOptions();

		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		public FluentHttpClientBuilder(
			IServiceProvider serviceProvider,
			IFluentHttpClientFactory fluentHttpClientFactory,
			FluentHttpMiddlewareBuilder middlewareBuilder
		)
		{
			_serviceProvider = serviceProvider;
			_fluentHttpClientFactory = fluentHttpClientFactory;
			_middlewareBuilder = middlewareBuilder;
		}

		/// <summary>
		/// Set base url for each request.
		/// </summary>
		/// <param name="url">Base url address to set. e.g. "https://api.sketch7.com"</param>
		/// <param name="replace">Determines whether to replace the Url or appends a path to the current BaseUrl.</param>
		/// <returns>Returns client builder for chaining.</returns>
		public FluentHttpClientBuilder WithBaseUrl(string url, bool replace = true)
		{
			var trimmedUrl = $"{url.Trim(' ', '/')}/";

			_baseUrl = replace || string.IsNullOrEmpty(_baseUrl)
				? _baseUrl = trimmedUrl
				: _baseUrl + trimmedUrl;

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
			_headers.Set(key, value);
			return this;
		}

		/// <inheritdoc />
		public FluentHttpClientBuilder WithHeader(string key, StringValues values)
		{
			_headers.Set(key, values);
			return this;
		}

		/// <inheritdoc />
		public FluentHttpClientBuilder WithHeaders(IDictionary<string, string> headers)
		{
			_headers.SetRange(headers);
			return this;
		}

		/// <inheritdoc />
		public FluentHttpClientBuilder WithHeaders(IDictionary<string, string[]> headers)
		{
			_headers.SetRange(headers);
			return this;
		}

		/// <inheritdoc />
		public FluentHttpClientBuilder WithHeaders(IDictionary<string, StringValues> headers)
		{
			_headers.SetRange(headers);
			return this;
		}

		/// <inheritdoc />
		public FluentHttpClientBuilder WithHeaders(FluentHttpHeaders headers)
		{
			_headers.SetRange(headers);
			return this;
		}

		/// <summary>
		/// Set the identifier (unique key) for the HTTP client.
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
		/// <param name="replace">Determine whether invoking this again will replace previous defaults or be combined (defaults to combine).</param>
		/// <returns>Returns client builder for chaining.</returns>
		public FluentHttpClientBuilder WithRequestBuilderDefaults(Action<FluentHttpRequestBuilder> requestBuilderDefaults, bool replace = false)
		{
			if (replace)
				_requestBuilderDefaults = requestBuilderDefaults;
			else
				_requestBuilderDefaults += requestBuilderDefaults;

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
			_middlewareBuilder.Add(middleware, args);
			return this;
		}

		/// <summary>
		/// Build up HTTP client options.
		/// </summary>
		/// <returns>Returns HTTP client options.</returns>
		public FluentHttpClientOptions BuildOptions()
		{
			_formatterOptions.Resort();
			return new FluentHttpClientOptions
			{
				Timeout = _timeout,
				BaseUrl = _baseUrl,
				Identifier = Identifier,
				Headers = _headers,
				MiddlewareBuilder = _middlewareBuilder,
				RequestBuilderDefaults = _requestBuilderDefaults,
				HttpMessageHandler = _httpMessageHandler,
				Formatters = _formatterOptions.Formatters,
				DefaultFormatter = _formatterOptions.Default
			};
		}

		/// <summary>
		/// Builds a new HTTP client (with default <see cref="FluentHttpClient"/> implementation).
		/// </summary>
		/// <param name="options"></param>
		public IFluentHttpClient Build(FluentHttpClientOptions options = null)
			=> Build<FluentHttpClient>(options);

		/// <summary>
		/// Build a new HTTP client.
		/// </summary>
		/// <typeparam name="THttpClient">HttpClient type</typeparam>
		/// <param name="options"></param>
		public IFluentHttpClient Build<THttpClient>(FluentHttpClientOptions options = null)
			where THttpClient : IFluentHttpClient
		{
			if (options == null)
				options = BuildOptions();

			if (string.IsNullOrEmpty(options.Identifier))
				throw ClientBuilderValidationException.FieldNotSpecified(nameof(options.Identifier));

			if (string.IsNullOrEmpty(options.BaseUrl))
				throw ClientBuilderValidationException.FieldNotSpecified(nameof(options.BaseUrl));

			return ActivatorUtilities.CreateInstance<THttpClient>(_serviceProvider, options, _fluentHttpClientFactory);
		}

		/// <summary>
		/// Register to factory <see cref="IFluentHttpClientFactory"/> and build, same as <see cref="IFluentHttpClientFactory.Add(FluentHttpClientBuilder)"/> for convenience.
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
			_middlewareBuilder.AddRange(options.MiddlewareBuilder.GetAll());
			_requestBuilderDefaults = options.RequestBuilderDefaults;
			_httpMessageHandler = options.HttpMessageHandler;
			var formatters = options.Formatters.Union(_formatterOptions.Formatters, MediaTypeFormatterComparer.Instance).ToList();
			_formatterOptions.Formatters.Clear();
			_formatterOptions.Formatters.AddRange(formatters);
			_formatterOptions.Default = options.DefaultFormatter;

			return this;
		}
	}

	internal class MediaTypeFormatterComparer : IEqualityComparer<MediaTypeFormatter>
	{
		public static MediaTypeFormatterComparer Instance = new MediaTypeFormatterComparer();

		public bool Equals(MediaTypeFormatter x, MediaTypeFormatter y) => x?.GetType() == y?.GetType();

		public int GetHashCode(MediaTypeFormatter obj) => obj.GetType().GetHashCode();
	}
}