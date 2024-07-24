using System.Reflection;

namespace FluentlyHttpClient;

/// <summary>
/// Get the meta information about the library.
/// </summary>
public static class FluentlyHttpClientMeta
{
	/// <summary>
	/// Gets the version of the FluentlyHttpClient library.
	/// </summary>
	public static Version Version = typeof(FluentlyHttpClientMeta).GetTypeInfo()
		.Assembly.GetName().Version!;
}