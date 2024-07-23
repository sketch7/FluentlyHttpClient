using System.Net;
using System.Text.Json;

namespace FluentlyHttpClient.MediaFormatters;

// todo: move to separate lib?
public class SystemTextJsonMediaTypeFormatter : MediaTypeFormatter
{
	private const string MediaType = "application/json";
	private readonly JsonSerializerOptions _options;

	/// <summary>
	/// Initializes a new instance with default formatter resolver.
	/// </summary>
	public SystemTextJsonMediaTypeFormatter()
		: this(null)
	{
	}

	/// <summary>
	/// Initializes a new instance with the provided formatter resolver.
	/// </summary>
	/// <param name="options"></param>
	public SystemTextJsonMediaTypeFormatter(JsonSerializerOptions? options)
	{
		SupportedMediaTypes.Add(new(MediaType));
		_options = options ?? new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		};
	}

	public override bool CanReadType(Type type)
	{
		ArgumentNullException.ThrowIfNull(type, nameof(type));
		return IsAllowedType(type);
	}

	public override bool CanWriteType(Type type)
	{
		ArgumentNullException.ThrowIfNull(type, nameof(type));
		return IsAllowedType(type);
	}

	private static bool IsAllowedType(Type t)
	{
		if (t is { IsAbstract: false, IsInterface: false, IsNotPublic: false })
			return true;

		if (typeof(IEnumerable<>).IsAssignableFrom(t))
			return true;

		return false;
	}

	public override async Task<object?> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
		=> await JsonSerializer.DeserializeAsync(readStream, type, _options);

	public override async Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content,
		TransportContext transportContext)
		=> await JsonSerializer.SerializeAsync(writeStream, value, type, _options);
}