namespace FluentlyHttpClient.Entity;

/// <summary>Internal constants used for EF Core schema configuration.</summary>
public static class Constants
{
	/// <summary>Max length for short text columns (30).</summary>
	public const int ShortTextLength = 30;
	/// <summary>Max length for normal text columns (70).</summary>
	public const int NormalTextLength = 70;
	/// <summary>Max length for long text columns (1500).</summary>
	public const int LongTextLength = 1500;

	/// <summary>SQL schema name for cache tables.</summary>
	public const string SchemaName = "cache";

	/// <summary>Table name for HTTP response cache entries.</summary>
	public const string HttpResponseTable = "HttpResponses";
}