using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

// ReSharper disable once CheckNamespace
namespace FluentlyHttpClient
{
	/// <summary>
	/// Extensions for object.
	/// </summary>
	public static class ObjectExtensions
	{
		/// <summary>Get the key=>value pairs represented by a dictionary or anonymous object.</summary>
		/// <param name="arguments">The key=>value pairs in the query argument. If this is a dictionary, the keys and values are used. Otherwise, the property names and values are used.</param>
		public static IDictionary<string, object> ToDictionary(this object arguments)
		{
			switch (arguments)
			{
				case null:
					return new Dictionary<string, object>();
				case IDictionary<string, object> args:
					return args;
				case IDictionary argDict:
					IDictionary<string, object> dict = new Dictionary<string, object>();
					foreach (var key in argDict.Keys)
						dict.Add(key.ToString(), argDict[key]);
					return dict;
			}
			// object
			return arguments.GetType()
				.GetProperties()
				.Where(p => p.CanRead && p.GetCustomAttribute<IgnoreDataMemberAttribute>() == null)
				.ToDictionary(p => p.Name, p => p.GetValue(arguments));
		}

		/// <summary>
		/// Get the collection as a formatted string e.g. 'User-Agent=google&amp;Locale=en,es'.
		/// </summary>
		/// <param name="collection">Collection to format to string.</param>
		public static string ToFormattedString(this IEnumerable<KeyValuePair<string, IEnumerable<string>>> collection)
			=> collection.StringifyCollection();

		/// <summary>
		/// Get the collection as a formatted string e.g. 'User-Agent=google&amp;Locale=en,es'.
		/// </summary>
		/// <param name="collection">Collection to format to string.</param>
		public static string ToFormattedString(this IEnumerable<KeyValuePair<string, string[]>> collection)
			=> collection.StringifyCollection();

		private static string StringifyCollection<T>(this IEnumerable<KeyValuePair<string, T>> headers)
			where T : IEnumerable<string>
		{
			var concatHeaders = "";
			foreach (var header in headers)
				concatHeaders += $"{header.Key}={string.Join(",", header.Value)}&";

			concatHeaders = concatHeaders.TrimEnd('&');
			return concatHeaders;
		}
	}
}
