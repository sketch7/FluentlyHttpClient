using System;

namespace FluentlyHttpClient
{
	/// <summary>
	/// FluentHttpClientFactory extensions.
	/// </summary>
	public static class FluentHttpClientFactoryExtensions
	{
		/// <summary>
		/// Add/register HTTP client from options.
		/// </summary>
		/// <param name="factory"></param>
		/// <param name="options">options to register.</param>
		/// <returns>Returns HTTP client created.</returns>
		public static IFluentHttpClient Add(this IFluentHttpClientFactory factory, FluentHttpClientOptions options)
		{
			if (options == null) throw new ArgumentNullException(nameof(options));

			var builder = factory.CreateBuilder(options.Identifier)
				.FromOptions(options);
			return factory.Add(builder);
		}
	}
}