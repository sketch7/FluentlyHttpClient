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
				.Withdentifier(identifier);
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

		/// <summary>
		/// Remove/unregister Http Client.
		/// </summary>
		/// <param name="identity">Identity to remove.</param>
		/// <returns></returns>
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
}