using System.Web;

namespace FluentlyHttpClient;

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
public record QueryStringOptions
{
	private static readonly Func<string, string> CamelCaseString = key => key.ToCamelCase();
	/// <summary>
	/// Default KeyFormatter.
	/// </summary>
	public static readonly Func<string, string> DefaultKeyFormatter = CamelCaseString;

	private static readonly Func<string, string> HttpEncode = HttpUtility.UrlEncode;

	/// <summary>
	/// Default ValueEncoder.
	/// </summary>
	public static readonly Func<string, string> DefaultValueEncoder = HttpEncode;

	protected string DebuggerDisplay => $"CollectionMode: '{CollectionMode}'";

	/// <summary>
	/// Get or set the query string collection mode to format.
	/// </summary>
	public QueryStringCollectionMode CollectionMode { get; set; }

	/// <summary>
	/// Gets or sets the function to format a collection item. This will allow you to manipulate the value.
	/// </summary>
	public Func<object, string>? CollectionItemFormatter { get; set; }
	public Dictionary<string, Func<object, string>>? CollectionKeyValueItemFormatter { get; set; }

	/// <summary>
	/// Gets or sets the function to format the key e.g. lowercase.
	/// </summary>
	internal Func<string, string> KeyFormatter { get; set; } = DefaultKeyFormatter;

	internal Func<object, string>? ValueFormatter { get; set; }
	internal Dictionary<string, Func<object, string>>? KeyValueFormatter { get; set; }

	internal Func<string, string> ValueEncoder { get; set; } = DefaultValueEncoder;

	internal Func<string, string>? CollectionKeyFormatter { get; set; }

	/// <summary>
	/// Gets or sets the function to format a collection item. This will allow you to manipulate the value.
	/// </summary>
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
	/// Gets or sets the function to format the key to be used only for collections. Useful to make php like keys e.g. append '[]' 'filter[]'.
	/// NOTE: <see cref="WithKeyFormatter"/> will still be used.
	/// </summary>
	public QueryStringOptions WithCollectionKeyFormatter(Func<string, string> configure)
	{
		CollectionKeyFormatter = configure;
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
	/// Gets or sets the function to format a value per key. This will allow you to manipulate the value by key
	/// </summary>
	/// <param name="configure"></param>
	/// <returns></returns>
	public QueryStringOptions WithValuePerKeyFormatter(Dictionary<string, Func<object, string>> configure)
	{
		KeyValueFormatter = configure;
		CollectionKeyValueItemFormatter = configure;
		return this;
	}

	/// <summary>
	/// Gets or sets the function to encode a value. This will allow you to encode the value. e.g. UrlEncode.
	/// </summary>
	public QueryStringOptions WithValueEncoder(Func<string, string> configure)
	{
		ValueEncoder = configure;
		return this;
	}
}