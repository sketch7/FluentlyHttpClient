using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

// ReSharper disable once CheckNamespace
namespace FluentlyHttpClient
{
	/// <summary>
	/// Extension for regex.
	/// </summary>
	public static class RegexExtensions
	{
		/// <summary>
		/// Replaces string tokens with arguments (interpolation).
		/// </summary>
		/// <param name="re">regex instance.</param>
		/// <param name="template">Template used for replacement/interpolation. e.g. <c>"/person/{id}"</c></param>
		/// <param name="args">Arguments to interpolate with template.</param>
		/// <returns>Returns string with tokens replaced.</returns>
		public static string ReplaceTokens(this Regex re, string template, IDictionary<string, object> args)
		{
			string Evaluator(Match match)
			{
				var paramName = match.Groups[1].Value;
				var paramValue = args[paramName];
				if (paramValue == null)
					throw new ArgumentNullException(nameof(args), $"Template has a param which its value is not provided. Param: '{paramName}'");
				return args[match.Groups[1].Value].ToString();
			}

			return re.Replace(template, Evaluator);
		}
	}
}