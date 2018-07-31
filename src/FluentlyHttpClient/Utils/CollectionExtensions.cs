using System.Collections;
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
			string AddQueryString(string key, string value, string uri)
			{
				if (string.IsNullOrEmpty(value)) return value;

				if (uri.Length > 0)
					uri = $"{uri}&";

				uri = $"{uri}{key}={HttpUtility.UrlEncode(value)}";
				return uri;
			}

			if (dict == null || dict.Count == 0)
				return string.Empty;

			var qs = string.Empty;
			foreach (var item in dict)
			{
				if (item.Value == null)
					continue;

				if (item.Value is string)
				{
					qs = AddQueryString(item.Key.ToString(), item.Value.ToString(), qs);
					continue;
				}

				switch (item.Value)
				{
					case IEnumerable values:
						foreach (var value in values)
							qs = AddQueryString(item.Key.ToString(), value.ToString(), qs);
						break;
					
					default:
						qs = AddQueryString(item.Key.ToString(), item.Value.ToString(), qs);
						break;
				}
			}

			return qs;
		}
	}
}