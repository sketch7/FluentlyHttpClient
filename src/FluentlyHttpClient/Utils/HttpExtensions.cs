using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http.Headers;
using System.Web;

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

		/// <summary>
		/// Manipulate querystring collection.
		/// </summary>
		/// <param name="builder"></param>
		/// <param name="manipulateQueryString"></param>
		public static UriBuilder ManipulateQueryString(this UriBuilder builder, Action<NameValueCollection> manipulateQueryString)
		{
			if (string.IsNullOrEmpty(builder.Query))
				return builder;

			var queryString = HttpUtility.ParseQueryString(builder.Query);
			manipulateQueryString(queryString);

			builder.Query = queryString.ToString();

			return builder;
		}
	}
}
