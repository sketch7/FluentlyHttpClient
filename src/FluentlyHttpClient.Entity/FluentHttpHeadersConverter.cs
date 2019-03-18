using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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
		public static ValueConverter<FluentHttpHeaders, string> Convert = new ValueConverter<FluentHttpHeaders, string>(
			x => DataSerializer.Serialize(x.ToDictionary()),
			x => new FluentHttpHeaders(DataSerializer.Deserialize<Dictionary<string, string[]>>(x))
			);
	}
}