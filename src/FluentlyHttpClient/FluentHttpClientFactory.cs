using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace FluentlyHttpClient
{
	/// <summary>
	/// Class which contains registered <see cref="FluentHttpClient"/> and able to to get existing or creating new ones.
	/// </summary>
	public class FluentHttpClientFactory
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly Dictionary<string, FluentHttpClient> _clientsMap = new Dictionary<string, FluentHttpClient>();

		public FluentHttpClientFactory(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		/// <summary>
		/// Creates a new <see cref="FluentHttpClientBuilder"/>.
		/// </summary>
		/// <param name="identifier"></param>
		/// <returns></returns>
		public FluentHttpClientBuilder CreateBuilder(string identifier)
		{
			var clientBuilder = ActivatorUtilities.CreateInstance<FluentHttpClientBuilder>(_serviceProvider, this)
				.SetIdentifier(identifier);
			return clientBuilder;
		}

		/// <summary>
		/// Add/register Http Client from builder.
		/// </summary>
		/// <param name="clientBuilder">Client builder to register.</param>
		/// <returns>Returns </returns>
		public FluentHttpClient Add(FluentHttpClientBuilder clientBuilder)
		{
			if (clientBuilder == null) throw new ArgumentNullException(nameof(clientBuilder));

			if (string.IsNullOrEmpty(clientBuilder.Identifier))
				throw ClientBuilderValidationException.FieldNotSpecified(nameof(clientBuilder.Identifier));

			if (Has(clientBuilder.Identifier))
				throw new ClientBuilderValidationException($"FluentHttpClient '{clientBuilder.Identifier}' is already registered.");

			var clientOptions = clientBuilder.Build();
			SetDefaultOptions(clientOptions);

			if (string.IsNullOrEmpty(clientOptions.BaseUrl))
				throw ClientBuilderValidationException.FieldNotSpecified(nameof(clientOptions.BaseUrl));

			var client = ActivatorUtilities.CreateInstance<FluentHttpClient>(_serviceProvider, clientOptions);

			_clientsMap.Add(clientBuilder.Identifier, client);
			return client;
		}

		public FluentHttpClientFactory Remove(string identity)
		{
			_clientsMap.Remove(identity);
			// todo: dispose?
			return this;
		}

		/// <summary>
		/// Merge default options with the specified <see cref="options"/>.
		/// </summary>
		/// <param name="options">Options to check and merge with.</param>
		protected void SetDefaultOptions(FluentHttpClientOptions options)
		{
			if (options == null) throw new ArgumentNullException(nameof(options));
			if (options.Timeout == TimeSpan.Zero)
				options.Timeout = TimeSpan.FromSeconds(15);
		}

		/// <summary>
		/// Get <see cref="FluentHttpClient"/> registered by identifier.
		/// </summary>
		/// <param name="identifier">Identifier to get.</param>
		/// <exception cref="KeyNotFoundException">Throws an exception when key is not found.</exception>
		/// <returns></returns>
		public FluentHttpClient Get(string identifier)
		{
			if (!_clientsMap.TryGetValue(identifier, out var client))
				throw new KeyNotFoundException($"FluentHttpClient '{identifier}' not registered.");
			return client;
		}

		/// <summary>
		/// Determine whether identifier is already registered.
		/// </summary>
		/// <param name="identifier">Identifier to check.</param>
		/// <returns>Returns true when already exists.</returns>
		public bool Has(string identifier) => _clientsMap.ContainsKey(identifier);
	}

	/// <summary>
	/// Class to configure <see cref="FluentHttpClient"/> with a fluent API.
	/// </summary>
	public class FluentHttpClientBuilder
	{
		/// <summary>
		/// Gets the identifier specified.
		/// </summary>
		public string Identifier { get; private set; }

		private readonly FluentHttpClientFactory _fluentHttpClientFactory;
		private string _baseUrl;
		private TimeSpan _timeout;
		private readonly Dictionary<string, string> _headers = new Dictionary<string, string>();
		private readonly List<Type> _middleware = new List<Type>();

		public FluentHttpClientBuilder(FluentHttpClientFactory fluentHttpClientFactory)
		{
			_fluentHttpClientFactory = fluentHttpClientFactory;
		}

		public FluentHttpClientBuilder SetBaseUrl(string url)
		{
			_baseUrl = url;
			return this;
		}

		public FluentHttpClientBuilder SetTimeout(int timeout) => SetTimeout(TimeSpan.FromSeconds(timeout));

		public FluentHttpClientBuilder SetTimeout(TimeSpan timeout)
		{
			_timeout = timeout;
			return this;
		}

		public FluentHttpClientBuilder AddHeader(string key, string value)
		{
			if (_headers.ContainsKey(key))
				_headers[key] = value;
			else
				_headers.Add(key, value);
			return this;
		}

		public FluentHttpClientBuilder AddHeaders(IDictionary<string, string> headers)
		{
			foreach (var item in headers)
				AddHeader(item.Key, item.Value);
			return this;
		}

		public FluentHttpClientBuilder SetIdentifier(string identifier)
		{
			Identifier = identifier;
			return this;
		}

		/// <summary>
		/// Register middleware for the HttpClient, which each request pass-through.
		/// </summary>
		/// <typeparam name="T">Middleware type.</typeparam>
		/// <returns>Returns client builder for chaining.</returns>
		public FluentHttpClientBuilder AddMiddleware<T>() => AddMiddleware(typeof(T));

		/// <summary>
		/// Register middleware for the HttpClient, which each request pass-through.
		/// </summary>
		/// <returns>Returns client builder for chaining.</returns>
		public FluentHttpClientBuilder AddMiddleware(Type middleware)
		{
			_middleware.Add(middleware);
			return this;
		}

		/// <summary>
		/// Build up http client options.
		/// </summary>
		/// <returns>Returns http client options.</returns>
		public FluentHttpClientOptions Build()
		{
			var options = new FluentHttpClientOptions
			{
				Timeout = _timeout,
				BaseUrl = _baseUrl,
				Identifier = Identifier,
				Headers = _headers,
				Middleware = _middleware
			};
			return options;
		}

		/// <summary>
		/// Register to <see cref="FluentHttpClientFactory"/>, same as <see cref="FluentHttpClientFactory.Add"/> for convience.
		/// </summary>
		public FluentHttpClientBuilder Register()
		{
			_fluentHttpClientFactory.Add(this);
			return this;
		}
	}
}