using System;
using System.Diagnostics;

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
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class QueryStringOptions
	{
		private static readonly Func<string, string> CamelCaseString = key => key.ToCamelCase();
		/// <summary>
		/// Default KeyFormatter.
		/// </summary>
		public static readonly Func<string, string> DefaultKeyFormatter = CamelCaseString;

		/// <summary>
		/// Debugger display.
		/// </summary>
		protected string DebuggerDisplay => $"CollectionMode: '{CollectionMode}'";

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
		public Func<string, string> KeyFormatter { get; set; } = DefaultKeyFormatter;
	}
}