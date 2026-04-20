using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace FluentlyHttpClient.Entity;

/// <summary>JSON serialization helpers for EF Core value conversions.</summary>
public static class DataSerializer
{
	private static readonly JsonSerializerSettings Settings = new()
	{
		ContractResolver = new DefaultContractResolver()
	};

	/// <summary>Serialize <paramref name="value"/> to a JSON string.</summary>
	public static string Serialize<TItem>(TItem value) => JsonConvert.SerializeObject(value, Settings);
	/// <summary>Deserialize a JSON string to <typeparamref name="TResult"/>.</summary>
	public static TResult Deserialize<TResult>(string value) =>
		// JsonConvert.DeserializeObject returns non-null when given a valid JSON string and valid target type
		JsonConvert.DeserializeObject<TResult>(value, Settings)!;
}