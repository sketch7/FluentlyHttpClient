using System;
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
		[Obsolete("Instead simply use dic[key] = value")]
		public static void Set<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue value) => dict[key] = value;
	}
}