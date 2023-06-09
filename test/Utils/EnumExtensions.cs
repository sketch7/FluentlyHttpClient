using System.ComponentModel;
using System.Reflection;

namespace FluentlyHttpClient.Test.Utils;

public static class EnumExtensions
{
	public static string GetEnumDescription(this Enum value)
	{
		var fi = value.GetType().GetField(value.ToString());

		var attributes = fi?.GetCustomAttribute<DescriptionAttribute>(false);

		if (attributes != null)
			return attributes.Description;
		return value.ToString();
	}
}