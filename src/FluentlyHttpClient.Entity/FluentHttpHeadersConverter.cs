using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace FluentlyHttpClient.Entity
{
	//public class FluentHttpHeadersConverter : ValueConverter<FluentHttpHeaders, string>
	//{
	//	private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings();

	//	public FluentHttpHeadersConverter(ConverterMappingHints hints = default) :
	//		base
	//			(x => JsonConvert.SerializeObject(x, Settings),
	//			x => new FluentHttpHeaders(JsonConvert.DeserializeObject<Dictionary<string, string[]>>(x, Settings))
	//			, hints
	//			)
	//	{ }
	//}

	public static class FluentHttpHeadersConversion
	{
		private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() };

		public static ValueConverter<FluentHttpHeaders, string> Convert = new ValueConverter<FluentHttpHeaders, string>(
			x => JsonConvert.SerializeObject(x, Settings),
			x => new FluentHttpHeaders(JsonConvert.DeserializeObject<Dictionary<string, string[]>>(x, Settings), null));
	}
}