using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;

// ReSharper disable once CheckNamespace
namespace FluentlyHttpClient
{
	public static class HttpExtensions
	{
		/// <summary>
		/// Copy headers from source
		/// </summary>
		/// <param name="destination">Headers to copy to.</param>
		/// <param name="source">Headers to copy from.</param>
		public static void CopyFrom(this HttpHeaders destination, HttpHeaders source)
		{
			foreach (var header in source)
				destination.TryAddWithoutValidation(header.Key, header.Value);
		}

		/// <summary>
		/// Converts from HttpHeaders dictionary.
		/// </summary>
		/// <param name="headers"></param>
		/// <returns></returns>
		public static Dictionary<string, string> ToDictionary(this HttpHeaders headers)
			=> headers.ToDictionary(x => x.Key, x => string.Join(";", x.Value));

		/// <summary>
		/// Add from dictionary.
		/// </summary>
		/// <param name="headers"></param>
		/// <param name="values">Headers to add from.</param>
		/// <returns></returns>
		public static void AddRange(this HttpHeaders headers, Dictionary<string, string> values)
		{
			foreach (var headerEntry in values)
				headers.Add(headerEntry.Key, headerEntry.Value);
		}

		/// <summary>
		/// Converts headers dictionary to hash string.
		/// </summary>
		/// <param name="headers"></param>
		/// <returns></returns>
		public static string ToHeadersHashString(this Dictionary<string, string> headers)
		{
			var headersHash = "";
			foreach (var header in headers)
				headersHash += $"{header.Key}={header.Value}&";
			headersHash = headersHash.TrimEnd('&');
			return headersHash;
		}
	}
}
