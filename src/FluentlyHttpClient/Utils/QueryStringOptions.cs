﻿using System;
using System.Diagnostics;
#pragma warning disable 618 // todo: remove after removing deprecated code

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
		[Obsolete("Use WithValueFormatter instead.")] // deprecated: remove
		public Func<object, string> CollectionItemFormatter { get; set; }

		/// <summary>
		/// Gets or sets the function to format the key e.g. lowercase.
		/// </summary>
		[Obsolete("Use WithKeyFormatter instead.")] // deprecated: make internal
		public Func<string, string> KeyFormatter { get; set; } = DefaultKeyFormatter;

		internal Func<object, string> ValueFormatter { get; set; }

		/// <summary>
		/// Gets or sets the function to format a collection item. This will allow you to manipulate the value.
		/// </summary>
		[Obsolete("Use WithValueFormatter instead. ValueFormatter will configure collection items and even props.")] // deprecated: remove
		public QueryStringOptions WithCollectionItemFormatter(Func<object, string> configure)
		{
			CollectionItemFormatter = configure;
			return this;
		}

		/// <summary>
		/// Gets or sets the function to format the key e.g. lowercase.
		/// </summary>
		public QueryStringOptions WithKeyFormatter(Func<string, string> configure)
		{
			KeyFormatter = configure;
			return this;
		}

		/// <summary>
		/// Gets or sets the function to format a value. This will allow you to manipulate the value e.g. use enums attribute instead of value.
		/// </summary>
		public QueryStringOptions WithValueFormatter(Func<object, string> configure)
		{
			ValueFormatter = configure;
			CollectionItemFormatter = configure;
			return this;
		}

		/// <summary>
		/// Clones options into a new instance.
		/// </summary>
		/// <returns>Returns new instance with the copied options.</returns>
		public QueryStringOptions Clone()
			=> new QueryStringOptions
			{
				CollectionMode = CollectionMode,
				CollectionItemFormatter = CollectionItemFormatter,
				KeyFormatter = KeyFormatter,
				ValueFormatter = ValueFormatter,
			};
	}
}