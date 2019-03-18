using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace FluentlyHttpClient.Entity
{
	public static class DataSerializer
	{
		private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() };

		public static string Serialize<TItem>(TItem value) => JsonConvert.SerializeObject(value, Settings);
		public static TResult Deserialize<TResult>(string value) => JsonConvert.DeserializeObject<TResult>(value, Settings);

		//public static string Serialize<TItem>(TItem value)
		//{
		//	var data = MessagePack.MessagePackSerializer.Serialize(value, MessagePack.Resolvers.ContractlessStandardResolver.Instance);
		//	var result = System.Text.Encoding.UTF8.GetString(data);
		//	return result;
		//}

		//public static TResult Deserialize<TResult>(string value)
		//{
		//	var data = System.Text.Encoding.UTF8.GetBytes(value);
		//	var result = MessagePack.MessagePackSerializer.Deserialize<TResult>(data, MessagePack.Resolvers.ContractlessStandardResolver.Instance);
		//	return result;
		//}
	}
}