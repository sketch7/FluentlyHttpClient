using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
				.GetRuntimeProperties()
				.Where(p => p.CanRead)
				.ToDictionary(p => p.Name, p => p.GetValue(arguments));
		}
	}
}
