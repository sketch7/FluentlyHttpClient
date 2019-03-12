using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace FluentlyHttpClient
{
	/// <summary>
	/// HTTP client factory which contains registered HTTP clients and able to get existing or creating new ones.
	/// </summary>
	public interface IFluentHttpClientFactory
	{
		/// <summary>
		/// Creates a new <see cref="FluentHttpClientBuilder"/>.
		/// </summary>
		/// <param name="identifier">identifier to set.</param>
		/// <returns>Returns a new HTTP client builder.</returns>
		FluentHttpClientBuilder CreateBuilder(string identifier);

		/// <summary>
		/// Configure defaults for <see cref="FluentHttpClientBuilder"/> which every new one uses.
		/// </summary>
		/// <param name="configure">Configuration function.</param>
		/// <returns>Returns client factory for chaining.</returns>
		IFluentHttpClientFactory ConfigureDefaults(Action<FluentHttpClientBuilder> configure);

		/// <summary>
		/// Get <see cref="IFluentHttpClient"/> registered by identifier.
		/// </summary>
		/// <param name="identifier">Identifier to get.</param>
		/// <exception cref="KeyNotFoundException">Throws an exception when key is not found.</exception>
		/// <returns>Returns HTTP client.</returns>
		IFluentHttpClient Get(string identifier);

		/// <summary>
		/// Add/register HTTP client.
		/// </summary>
		/// <param name="client">Client to register.</param>
		/// <returns>Returns HTTP client.</returns>
		IFluentHttpClient Add(IFluentHttpClient client);

		/// <summary>
		/// Add/register HTTP client from builder.
		/// </summary>
		/// <param name="clientBuilder">Client builder to register.</param>
		/// <returns>Returns HTTP client created.</returns>
		IFluentHttpClient Add(FluentHttpClientBuilder clientBuilder);

		/// <summary>
		/// Remove/unregister HTTP client.
		/// </summary>
		/// <param name="identifier">Identity to remove.</param>
		/// <returns>Returns client factory for chaining.</returns>
		IFluentHttpClientFactory Remove(string identifier);

		/// <summary>
		/// Determine whether identifier is already registered.
		/// </summary>
		/// <param name="identifier">Identifier to check.</param>
		/// <returns>Returns true when already exists.</returns>
		bool Has(string identifier);
	}

	/// <summary>
	/// Class which contains registered <see cref="IFluentHttpClient"/> and able to get existing or creating new ones.
	/// </summary>
	public class FluentHttpClientFactory : IFluentHttpClientFactory
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly Dictionary<string, IFluentHttpClient> _clientsMap = new Dictionary<string, IFluentHttpClient>();
		private Action<FluentHttpClientBuilder> _configure;

		/// <summary>
		/// Initializes a new instance of <see cref="FluentHttpClientFactory"/>.
		/// </summary>
		/// <param name="serviceProvider"></param>
		public FluentHttpClientFactory(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		/// <inheritdoc />
		public FluentHttpClientBuilder CreateBuilder(string identifier)
		{
			var clientBuilder = ActivatorUtilities.CreateInstance<FluentHttpClientBuilder>(_serviceProvider, this)
				.WithIdentifier(identifier)
				.WithUserAgent("fluently")
				.WithTimeout(15);

			_configure?.Invoke(clientBuilder);

			return clientBuilder;
		}

		/// <inheritdoc />
		public IFluentHttpClientFactory ConfigureDefaults(Action<FluentHttpClientBuilder> configure)
		{
			_configure = configure;
			return this;
		}

		/// <inheritdoc />
		public IFluentHttpClient Get(string identifier)
		{
			if (!_clientsMap.TryGetValue(identifier, out var client))
				throw new KeyNotFoundException($"FluentHttpClient '{identifier}' not registered.");
			return client;
		}

		/// <inheritdoc />
		public IFluentHttpClient Add(IFluentHttpClient client)
		{
			if (client == null) throw new ArgumentNullException(nameof(client));

			if (Has(client.Identifier))
				throw new ClientBuilderValidationException($"FluentHttpClient '{client.Identifier}' is already registered.");
			_clientsMap.Add(client.Identifier, client);
			return client;
		}

		/// <inheritdoc />
		public IFluentHttpClient Add(FluentHttpClientBuilder clientBuilder)
		{
			if (clientBuilder == null) throw new ArgumentNullException(nameof(clientBuilder));

			var client = clientBuilder.Build();
			return Add(client);
		}

		/// <inheritdoc />
		public IFluentHttpClientFactory Remove(string identifier)
		{
			if (!_clientsMap.TryGetValue(identifier, out var client))
				return this;

			_clientsMap.Remove(identifier);
			client.Dispose();
			return this;
		}

		/// <inheritdoc />
		public bool Has(string identifier) => _clientsMap.ContainsKey(identifier);
	}
}