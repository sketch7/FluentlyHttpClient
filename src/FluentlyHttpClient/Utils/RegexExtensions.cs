using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace FluentlyHttp.Utils
{
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
			=> re.Replace(template, match => args[match.Groups[1].Value].ToString());
	}
}