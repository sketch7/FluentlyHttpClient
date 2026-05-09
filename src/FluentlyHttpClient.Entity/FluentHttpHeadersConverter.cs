using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FluentlyHttpClient.Entity;

/// <summary>EF Core value conversion helpers for <see cref="FluentHttpHeaders"/>.</summary>
public static class FluentHttpHeadersConversion
{
	/// <summary>Converter that serializes <see cref="FluentHttpHeaders"/> to/from a JSON string.</summary>
	public static ValueConverter<FluentHttpHeaders?, string> Convert = new(
		x => x != null ? DataSerializer.Serialize(x) : string.Empty,
		x => DataSerializer.Deserialize<FluentHttpHeaders>(x)
	);
}