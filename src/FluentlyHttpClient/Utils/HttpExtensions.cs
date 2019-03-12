using System.Collections.Generic;
using System.Net.Http.Headers;

// ReSharper disable once CheckNamespace
namespace FluentlyHttpClient
{
	/// <summary>
	/// HTTP extensions such as HttpHeaders.
	/// </summary>
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
		/// Add all from fluent HTTP headers.
		/// </summary>
		/// <param name="headers">Headers to add to.</param>
		/// <param name="values">Headers to add from.</param>
		public static void AddRange(this HttpHeaders headers, FluentHttpHeaders values)
		{
			foreach (var headerEntry in values)
				headers.Add(headerEntry.Key, (IEnumerable<string>)headerEntry.Value);
		}
	}
}
