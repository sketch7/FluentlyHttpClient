using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace FluentlyHttpClient.Entity
{
	public static class DataSerializer
	{
		private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
		{
			ContractResolver = new DefaultContractResolver()
		};

		public static string Serialize<TItem>(TItem value) => JsonConvert.SerializeObject(value, Settings);
		public static TResult Deserialize<TResult>(string value) => JsonConvert.DeserializeObject<TResult>(value, Settings);
	}
}