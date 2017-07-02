using System.Collections.Generic;

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
		public static void Set<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue value)
		{
			if (dict.ContainsKey(key))
				dict[key] = value;
			else
				dict.Add(key, value);
		}
	}
}