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