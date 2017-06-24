using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace FluentlyHttp
{
	/*
	 * todo:
	 *  - FluentHttpClientFactory
	 *		- Default configs
	 *		- Default configs configurable
	 *  - 
	 */

	public class FluentHttpClientFactory
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly Dictionary<string, FluentHttpClient> _clientsMap = new Dictionary<string, FluentHttpClient>();

		public FluentHttpClientFactory(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public FluentHttpClientBuilder CreateBuilder(string identifier)
		{
			var builder = new FluentHttpClientBuilder(this);
			builder.SetIdentifier(identifier);
			return builder;
		}

		public FluentHttpClientFactory Add(FluentHttpClientBuilder clientBuilder)
		{
			if (clientBuilder == null) throw new ArgumentNullException(nameof(clientBuilder));
			if (Has(clientBuilder.Identifier))
				throw new KeyNotFoundException($"FluentHttpClient '{clientBuilder.Identifier}' is already registered.");

			var clientOptions = clientBuilder.Build();
			var client = ActivatorUtilities.CreateInstance<FluentHttpClient>(_serviceProvider, clientOptions);

			_clientsMap.Add(clientBuilder.Identifier, client);
			return this;
		}

		public FluentHttpClientFactory Remove(string identity)
		{
			_clientsMap.Remove(identity);
			// todo: dispose?
			return this;
		}

		public FluentHttpClient Get(string identifier)
		{
			if (!_clientsMap.TryGetValue(identifier, out var client))
				throw new KeyNotFoundException($"FluentHttpClient '{identifier}' not registered.");
			return client;
		}

		public bool Has(string identifier) => _clientsMap.ContainsKey(identifier);
	}

	public class FluentHttpClientBuilder
	{
		private readonly FluentHttpClientFactory _fluentHttpClientFactory;
		private string _baseUrl;
		private TimeSpan _timeout;
		public string Identifier { get; private set; }
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

		public FluentHttpClientBuilder SetTimeout(int timeout)
		{
			_timeout = TimeSpan.FromSeconds(timeout);
			return this;
		}
		public FluentHttpClientBuilder SetTimeout(TimeSpan timeout)
		{
			_timeout = timeout;
			return this;
		}

		public FluentHttpClientBuilder AddHeader(string key, string value)
		{
			_headers.Add(key, value);
			return this;
		}

		public FluentHttpClientBuilder SetIdentifier(string identifier)
		{
			Identifier = identifier;
			return this;
		}

		public FluentHttpClientBuilder AddMiddleware<T>()
		{
			_middleware.Add(typeof(T));
			return this;
		}

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
		/// Register to <see cref="FluentHttpClientFactory"/>, same as <see cref="Add(FluentHttpClientBuilder)"/>
		/// </summary>
		public FluentHttpClientBuilder Register()
		{
			_fluentHttpClientFactory.Add(this);
			return this;
		}
	}

}
