using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

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
		/// Get <see cref="IFluentHttpClient"/> registered by identifier.
		/// </summary>
		/// <param name="identifier">Identifier to get.</param>
		/// <exception cref="KeyNotFoundException">Throws an exception when key is not found.</exception>
		/// <returns>Returns HTTP client.</returns>
		IFluentHttpClient Get(string identifier);

		/// <summary>
		/// Add/register HTTP Client from options.
		/// </summary>
		/// <param name="options">options to register.</param>
		/// <returns>Returns HTTP client created.</returns>
		IFluentHttpClient Add(FluentHttpClientOptions options);

		/// <summary>
		/// Add/register HTTP Client from builder.
		/// </summary>
		/// <param name="clientBuilder">Client builder to register.</param>
		/// <returns>Returns HTTP client created.</returns>
		IFluentHttpClient Add(FluentHttpClientBuilder clientBuilder);

		/// <summary>
		/// Remove/unregister HTTP Client.
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

		public FluentHttpClientFactory(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		/// <summary>
		/// Creates a new <see cref="FluentHttpClientBuilder"/>.
		/// </summary>
		/// <param name="identifier">identifier to set.</param>
		/// <returns>Returns a new HTTP client builder.</returns>
		public FluentHttpClientBuilder CreateBuilder(string identifier)
		{
			var clientBuilder = ActivatorUtilities.CreateInstance<FluentHttpClientBuilder>(_serviceProvider, this)
				.Withdentifier(identifier);
			return clientBuilder;
		}

		/// <summary>
		/// Get <see cref="IFluentHttpClient"/> registered by identifier.
		/// </summary>
		/// <param name="identifier">Identifier to get.</param>
		/// <exception cref="KeyNotFoundException">Throws an exception when identifier is not found.</exception>
		/// <returns>Returns HTTP client.</returns>
		public IFluentHttpClient Get(string identifier)
		{
			if (!_clientsMap.TryGetValue(identifier, out var client))
				throw new KeyNotFoundException($"FluentHttpClient '{identifier}' not registered.");
			return client;
		}

		/// <summary>
		/// Add/register HTTP client from options.
		/// </summary>
		/// <param name="options">options to register.</param>
		/// <returns>Returns HTTP client created.</returns>
		public IFluentHttpClient Add(FluentHttpClientOptions options)
		{
			if (options == null) throw new ArgumentNullException(nameof(options));

			if (string.IsNullOrEmpty(options.Identifier))
				throw ClientBuilderValidationException.FieldNotSpecified(nameof(options.Identifier));

			if (Has(options.Identifier))
				throw new ClientBuilderValidationException($"FluentHttpClient '{options.Identifier}' is already registered.");

			SetDefaultOptions(options);

			if (string.IsNullOrEmpty(options.BaseUrl))
				throw ClientBuilderValidationException.FieldNotSpecified(nameof(options.BaseUrl));

			// todo: find a way how to use DI with additional param (or so) to factory for abstraction.
			var client = (IFluentHttpClient)ActivatorUtilities.CreateInstance<FluentHttpClient>(_serviceProvider, options);
			_clientsMap.Add(options.Identifier, client);
			return client;
		}

		/// <summary>
		/// Add/register HTTP Client from builder.
		/// </summary>
		/// <param name="clientBuilder">Client builder to register.</param>
		/// <returns>Returns HTTP client created.</returns>
		public IFluentHttpClient Add(FluentHttpClientBuilder clientBuilder)
		{
			if (clientBuilder == null) throw new ArgumentNullException(nameof(clientBuilder));
			var options = clientBuilder.Build();
			return Add(options);
		}

		/// <summary>
		/// Remove/unregister HTTP Client.
		/// </summary>
		/// <param name="identifier">Identity to remove.</param>
		/// <returns>Returns client factory for chaining.</returns>
		public IFluentHttpClientFactory Remove(string identifier)
		{
			if (!_clientsMap.TryGetValue(identifier, out var client))
				return this;

			_clientsMap.Remove(identifier);
			client.Dispose();
			return this;
		}

		/// <summary>
		/// Determine whether identifier is already registered.
		/// </summary>
		/// <param name="identifier">Identifier to check.</param>
		/// <returns>Returns true when already exists.</returns>
		public bool Has(string identifier) => _clientsMap.ContainsKey(identifier);

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
	}
}