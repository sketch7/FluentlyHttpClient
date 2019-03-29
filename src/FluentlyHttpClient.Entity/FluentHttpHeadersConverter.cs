using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FluentlyHttpClient.Entity
{
	public static class FluentHttpHeadersConversion
	{
		public static ValueConverter<FluentHttpHeaders, string> Convert = new ValueConverter<FluentHttpHeaders, string>(
			x => DataSerializer.Serialize(x),
			x => DataSerializer.Deserialize<FluentHttpHeaders>(x)
			);
	}
}