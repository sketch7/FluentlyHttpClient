using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;

// ReSharper disable once CheckNamespace
namespace FluentlyHttpClient
{
	/// <summary>
	/// Specify the querystring collection to be formatted as.
	/// </summary>
	public enum QueryStringCollectionMode
	{
		/// <summary>
		/// Specifies querystring collections are formatted key per value e.g. 'filter=assassin&amp;filter=fighter'.
		/// </summary>
		KeyPerValue = 0,

		/// <summary>
		/// Specifies querystring collections are formatted as comma separated e.g. 'filter=assassin,fighter'.
		/// </summary>
		CommaSeparated = 1
	}

	/// <summary>
	/// Querystring formatting options.
	/// </summary>
	public class QueryStringOptions
	{
		/// <summary>
		/// Get or set the query string collection mode to format.
		/// </summary>
		public QueryStringCollectionMode CollectionMode { get; set; }

		/// <summary>
		/// Gets or sets the function to format a collection item. This will allow you to manipulate the value.
		/// </summary>
		public Func<object, string> CollectionItemFormatter { get; set; }

		/// <summary>
		/// Gets or sets the function to format the key e.g. lowercase.
		/// </summary>
		public Func<string, string> KeyFormatter { get; set; }
	}

	/// <summary>
	/// Extensions for collections.
	/// </summary>
	public static class CollectionExtensions
	{
		private static readonly QueryStringOptions DefaultQueryStringOptions = new QueryStringOptions();

		/// <summary>
		/// Convert dictionary to querystring.
		/// </summary>
		/// <param name="dict">Dictionary</param>
		/// <param name="options">Formatting options.</param>
		/// <returns>Returns querystring.</returns>
		public static string ToQueryString<TKey, TValue>(this IDictionary<TKey, TValue> dict, QueryStringOptions options = null)
		{
			options = options ?? DefaultQueryStringOptions;

			if (dict == null || dict.Count == 0)
				return string.Empty;

			var qs = string.Empty;
			foreach (var item in dict)
			{
				if (item.Value == null)
					continue;

				var key = options.KeyFormatter != null
					? options.KeyFormatter(item.Key.ToString())
					: item.Key.ToString();

				if (item.Value is string)
				{
					qs = AddQueryString(key, item.Value.ToString(), qs);
					continue;
				}

				switch (item.Value)
				{
					case IEnumerable values:

						qs = BuildCollectionQueryString(key, values, qs, options);
						break;

					default:
						qs = AddQueryString(key, item.Value.ToString(), qs);
						break;
				}
			}

			return qs;
		}

		/// <summary>
		/// Convert dictionary to querystring.
		/// </summary>
		/// <param name="dict">Dictionary</param>
		/// <param name="configure">Configuration function.</param>
		/// <returns>Returns querystring.</returns>
		public static string ToQueryString<TKey, TValue>(this IDictionary<TKey, TValue> dict, Action<QueryStringOptions> configure)
		{
			if (configure == null) throw new ArgumentNullException(nameof(configure));
			var opts = new QueryStringOptions();
			configure(opts);
			return dict.ToQueryString(opts);
		}

		private static string BuildCollectionQueryString(string key, IEnumerable values, string qs, QueryStringOptions options)
		{
			switch (options.CollectionMode)
			{
				case QueryStringCollectionMode.KeyPerValue:
					foreach (var value in values)
					{
						var valueStr = options.CollectionItemFormatter != null
							? options.CollectionItemFormatter(value)
							: value.ToString();
						qs = AddQueryString(key, valueStr, qs);
					}
					break;
				case QueryStringCollectionMode.CommaSeparated:
					var index = 0;
					foreach (var value in values)
					{
						var valueStr = options.CollectionItemFormatter != null
							? options.CollectionItemFormatter(value)
							: value.ToString();
						if (index == 0)
							qs = AddQueryString(key, valueStr, qs);
						else
							qs += $",{HttpUtility.UrlEncode(valueStr)}";

						index++;
					}

					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(options), options.CollectionMode, $"Value provided {options.CollectionMode} is not supported.");
			}

			return qs;
		}

		private static string AddQueryString(string key, string value, string uri)
		{
			if (string.IsNullOrEmpty(value)) return value;

			if (uri.Length > 0) uri = $"{uri}&";

			uri = $"{uri}{key}={HttpUtility.UrlEncode(value)}";
			return uri;
		}
	}
}