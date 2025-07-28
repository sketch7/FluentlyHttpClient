using System.Collections;
using System.Web;

namespace FluentlyHttpClient;

/// <summary>
/// Extensions for collections.
/// </summary>
public static class QueryStringUtils
{
	private static readonly QueryStringOptions DefaultQueryStringOptions = new();

	/// <summary>
	/// Convert dictionary to querystring.
	/// </summary>
	/// <param name="dict">Dictionary</param>
	/// <param name="options">Formatting options.</param>
	/// <returns>Returns querystring.</returns>
	public static string ToQueryString<TKey, TValue>(this IDictionary<TKey, TValue>? dict, QueryStringOptions? options = null)
	{
		options ??= DefaultQueryStringOptions;

		if (dict == null || dict.Count == 0)
			return string.Empty;

		var qs = string.Empty;
		foreach (var item in dict)
		{
			if (item.Value == null)
				continue;

			var key = options.KeyFormatter != null
				? options.KeyFormatter(item.Key!.ToString()!)
				: item.Key!.ToString()!;

			if (item.Value is string)
			{
				qs = AddQueryString(key, FormatValue(key, item.Value), qs, options.ValueEncoder);
				continue;
			}

			switch (item.Value)
			{
				case IEnumerable values:
					qs = BuildCollectionQueryString(key, values, qs, options);
					break;
				default:
					qs = AddQueryString(key, FormatValue(key, item.Value), qs, options.ValueEncoder);
					break;
			}
		}

		return qs;

		string FormatValue(object param, object value)
		{
			if (options.ValueFormatter != null)
				return options.ValueFormatter(value);

			if (options.KeyValueFormatter != null && param is string && options.KeyValueFormatter.TryGetValue((param as string)!, out var transform))
				return transform(value);

			return value.ToString()!;
		}
	}

	/// <summary>
	/// Convert dictionary to querystring.
	/// </summary>
	/// <param name="dict">Dictionary</param>
	/// <param name="configure">Configuration function.</param>
	/// <returns>Returns querystring.</returns>
	public static string ToQueryString<TKey, TValue>(this IDictionary<TKey, TValue> dict, Action<QueryStringOptions> configure)
	{
		ArgumentNullException.ThrowIfNull(configure, nameof(configure));
		var opts = new QueryStringOptions();
		configure(opts);
		return dict.ToQueryString(opts);
	}

	private static string BuildCollectionQueryString(string key, IEnumerable values, string qs, QueryStringOptions options)
	{
		key = options.CollectionKeyFormatter == null
			? key
			: options.CollectionKeyFormatter(key);

		var collectionFormatter = options.CollectionItemFormatter;

		if (collectionFormatter == null && options.CollectionKeyValueItemFormatter?.TryGetValue(key, out var formatter) is true)
			collectionFormatter = formatter;

		switch (options.CollectionMode)
		{
			case QueryStringCollectionMode.KeyPerValue:
				foreach (var value in values)
				{
					var valueStr = collectionFormatter != null
						? collectionFormatter(value)
						: value.ToString();
					qs = AddQueryString(key, valueStr, qs, options.ValueEncoder);
				}
				break;
			case QueryStringCollectionMode.CommaSeparated:
				var index = 0;
				foreach (var value in values)
				{
					var valueStr = collectionFormatter != null
						? collectionFormatter(value)
						: value.ToString();
					if (index == 0)
						qs = AddQueryString(key, valueStr, qs, options.ValueEncoder);
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

	private static string AddQueryString(string key, string? value, string uri, Func<string, string> valueEncoder)
	{
		if (string.IsNullOrEmpty(value)) return uri;

		if (uri.Length > 0) uri = $"{uri}&";

		return $"{uri}{key}={valueEncoder(value)}";
	}
}