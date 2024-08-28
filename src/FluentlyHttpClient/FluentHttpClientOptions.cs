using FluentlyHttpClient.Middleware;
using Newtonsoft.Json.Serialization;

namespace FluentlyHttpClient;

/// <summary>
/// Options for <see cref="IFluentHttpClient"/>.
/// </summary>
public class FluentHttpClientOptions
{
	/// <summary>
	/// Gets or sets the base uri address for each request.
	/// </summary>
	public string? BaseUrl { get; set; }

	/// <summary>
	/// Gets or sets whether to use trailing slash or not for the base url.
	/// </summary>
	public bool? UseBaseUrlTrailingSlash { get; set; }

	/// <summary>
	/// Gets or sets the timespan to wait before the request times out.
	/// </summary>
	public TimeSpan Timeout { get; set; }

	/// <summary>
	/// Gets or sets the identifier (key) for the HTTP client.
	/// </summary>
	public string Identifier { get; set; } = null!;

	/// <summary>
	/// Gets or sets the headers which should be sent with each request.
	/// </summary>
	public FluentHttpHeaders Headers { get; set; } = null!;

	/// <summary>
	/// Gets or sets the middleware builder.
	/// </summary>
	public FluentHttpMiddlewareBuilder MiddlewareBuilder { get; set; } = null!;

	/// <summary>
	/// Gets or sets handler to customize request on creation. In order to specify defaults as desired, or so.
	/// </summary>
	public Action<FluentHttpRequestBuilder>? RequestBuilderDefaults { get; set; }

	/// <summary>
	/// Gets or sets HTTP handler stack to use for sending requests.
	/// </summary>
	public HttpMessageHandler? HttpMessageHandler { get; set; }

	/// <summary>
	/// Gets or sets formatters to be used for content negotiation, for "Accept" and body media formats. e.g. JSON, XML, etc...
	/// </summary>
	public MediaTypeFormatterCollection Formatters { get; set; } = null!;

	/// <summary>
	/// Gets or sets the default formatter to be used for content negotiation body format. e.g. JSON, XML, etc...
	/// </summary>
	public MediaTypeFormatter? DefaultFormatter { get; set; }

	/// <summary>
	/// Determine whether to auto register in factory.
	/// </summary>
	public bool AutoRegisterFactory { get; set; }
}

/// <summary>
/// Content negotiation formatter options. e.g. JSON, XML, etc...
/// </summary>
public class FormatterOptions
{
	/// <summary>
	/// Initializes formatter options with the default settings.
	/// </summary>
	public FormatterOptions()
	{
		Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
	}

	/// <summary>
	/// Configure formatters to be used.
	/// </summary>
	public MediaTypeFormatterCollection Formatters { get; } = [];

	/// <summary>
	/// Set default formatter to be used when serializing body content and the preferred "Accept".
	/// </summary>
	public MediaTypeFormatter? Default { get; set; }

	/// <summary>
	/// Resort formatters from the provided options.
	/// </summary>
	internal void Resort()
	{
		if (Default == null)
			return;

		var defaultFormatter = Formatters.FirstOrDefault(x => x == Default);
		if (defaultFormatter != null)
			Formatters.Remove(defaultFormatter);
		// place default formatter as first one so it will be preferred.
		Formatters.Insert(0, Default);
	}
}