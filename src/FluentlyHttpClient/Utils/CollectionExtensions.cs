using System;
using System.Collections.Generic;
using System.Web;

// ReSharper disable once CheckNamespace
namespace FluentlyHttpClient
{
	/// <summary>
	/// Extensions for collections.
	/// </summary>
	public static class CollectionExtensions
	{
		/// <summary>
		/// Add or update a key with the specified value.
		/// </summary>
		/// <param name="dict">Dictionary</param>
		/// <param name="key">Key to add/update.</param>
		/// <param name="value">Value to set.</param>
		[Obsolete("Instead simply use dic[key] = value")]
		public static void Set<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue value) => dict[key] = value;

		/// <summary>
		/// Convert dictionary to querystring.
		/// </summary>
		/// <param name="dict">Dictionary</param>
		/// <returns>Returns querystring.</returns>
		public static string ToQueryString<TKey, TValue>(this IDictionary<TKey, TValue> dict)
		{
			if (dict == null || dict.Count == 0)
				return "";

			var qs = "";
			foreach (var item in dict)
			{
				var value = item.Value?.ToString();
				if (string.IsNullOrEmpty(value)) continue;

				if (qs.Length > 0)
					qs = $"{qs}&";

				qs = $"{qs}{item.Key}={HttpUtility.UrlEncode(item.Value.ToString())}";
			}

			return qs;
		}
	}
}