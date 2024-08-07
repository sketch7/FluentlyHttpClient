using Microsoft.Extensions.Primitives;

namespace FluentlyHttpClient;

/// <summary>
/// Interface for Http header builder, which can be used to share extensions between
/// <see cref="FluentHttpClientBuilder"/> and <see cref="FluentHttpRequestBuilder"/>.
/// </summary>
/// <typeparam name="T">Builder type.</typeparam>
public interface IFluentHttpHeaderBuilder<out T>
{
	/// <summary>
	/// Add the specified header and its value for each request.
	/// </summary>
	/// <param name="key">Header to add.</param>
	/// <param name="value">Value for the header.</param>
	/// <returns>Returns client builder for chaining.</returns>
	T WithHeader(string key, string value);

	/// <summary>
	/// Add the specified header and its values for each request.
	/// </summary>
	/// <param name="key">Header to add.</param>
	/// <param name="values">Values for the header.</param>
	/// <returns>Returns client builder for chaining.</returns>
	T WithHeader(string key, StringValues values);

	/// <summary>
	/// Add the specified headers and their value for each request.
	/// </summary>
	/// <param name="headers">Headers to add.</param>
	/// <returns>Returns client builder for chaining.</returns>
	T WithHeaders(IDictionary<string, string> headers);

	/// <inheritdoc cref="WithHeaders(IDictionary{string,string})"/>
	T WithHeaders(IDictionary<string, string[]> headers);

	/// <inheritdoc cref="WithHeaders(IDictionary{string,string})"/>
	T WithHeaders(IDictionary<string, StringValues> headers);

	/// <inheritdoc cref="WithHeaders(IDictionary{string,string})"/>
	T WithHeaders(FluentHttpHeaders headers);
}

/// <summary>
/// Extensions for <see cref="IFluentHttpHeaderBuilder{T}"/> which are shared across client and request builders.
/// </summary>
public static class FluentHttpHeaderBuilderExtensions
{
	/// <summary>
	/// Set basic authentication header.
	/// </summary>
	/// <param name="builder">builder instance.</param>
	/// <param name="token">Auth token to add.</param>
	/// <returns>Returns request builder for chaining.</returns>
	public static T WithBasicAuthentication<T>(this IFluentHttpHeaderBuilder<T> builder, string token)
	{
		if (string.IsNullOrEmpty(token)) throw new ArgumentNullException(nameof(token));
		builder.WithHeader(HeaderTypes.Authorization, $"{AuthSchemeTypes.Basic} {token}");
		return (T)builder;
	}

	/// <summary>
	/// Set basic authentication header with username/password encoded to base64 (username:password).
	/// </summary>
	/// <param name="builder">builder instance.</param>
	/// <param name="username">Username to use for auth.</param>
	/// <param name="password">Password to use for auth.</param>
	/// <returns>Returns request builder for chaining.</returns>
	public static T WithBasicAuthentication<T>(this IFluentHttpHeaderBuilder<T> builder, string username, string password)
	{
		if (string.IsNullOrEmpty(username)) throw new ArgumentNullException(nameof(username));
		if (string.IsNullOrEmpty(password)) throw new ArgumentNullException(nameof(password));

		var token = Base64Encode($"{username}:{password}");
		return builder.WithBasicAuthentication(token);

		static string Base64Encode(string plainText)
		{
			var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
			return Convert.ToBase64String(plainTextBytes);
		}
	}

	/// <summary>
	/// Set bearer authentication header.
	/// </summary>
	/// <param name="builder">builder instance.</param>
	/// <param name="token">Auth token to add.</param>
	/// <returns>Returns request builder for chaining.</returns>
	public static T WithBearerAuthentication<T>(this IFluentHttpHeaderBuilder<T> builder, string token)
	{
		if (string.IsNullOrEmpty(token)) throw new ArgumentNullException(nameof(token));
		builder.WithHeader(HeaderTypes.Authorization, $"{AuthSchemeTypes.Bearer} {token}");
		return (T)builder;
	}

	/// <summary>
	/// Set user-agent header.
	/// </summary>
	/// <param name="builder">builder instance.</param>
	/// <param name="userAgent">User agent value to set.</param>
	/// <returns>Returns request builder for chaining.</returns>
	public static T WithUserAgent<T>(this IFluentHttpHeaderBuilder<T> builder, string userAgent)
	{
		if (string.IsNullOrEmpty(userAgent)) throw new ArgumentNullException(nameof(userAgent));
		builder.WithHeader(HeaderTypes.UserAgent, userAgent);
		return (T)builder;
	}
}