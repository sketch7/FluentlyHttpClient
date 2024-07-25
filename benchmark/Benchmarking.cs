using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using FluentlyHttpClient.MediaFormatters;
using FluentlyHttpClient.Middleware;
using FluentlyHttpClient.Test;
using MessagePack.Resolvers;
using Microsoft.Extensions.DependencyInjection;
using RichardSzalay.MockHttp;
using Serilog;
using Sketch7.MessagePack.MediaTypeFormatter;

namespace FluentlyHttpClient.Benchmarks;

[SimpleJob(RuntimeMoniker.Net80)]
[RPlotExporter, RankColumn]
[MemoryDiagnoser]
public class Benchmarking
{
	private IFluentHttpClient? _jsonHttpClient;
	private IFluentHttpClient? _systemTextJsonHttpClient;
	private IFluentHttpClient? _messagePackHttpClient;

	private readonly Hero _request = new()
	{
		Key = "valeera",
		Name = "Valeera",
		Title = "Shadow of the Uncrowned"
	};

	private IServiceProvider BuildContainer()
	{
		Log.Logger = new LoggerConfiguration()
			//.WriteTo.Console()
			//.WriteTo.Debug()
			.CreateLogger();
		var container = new ServiceCollection()
			.AddFluentlyHttpClient()
			.AddLogging(x => x.AddSerilog());
		return container.BuildServiceProvider();
	}

	[GlobalSetup]
	public void Setup()
	{
		var mockHttp = new MockHttpMessageHandler();
		mockHttp.When(HttpMethod.Post, "https://sketch7.com/api/json")
			.Respond("application/json", request => request.Content.ReadAsStreamAsync().Result);

		mockHttp.When(HttpMethod.Post, "https://sketch7.com/api/msgpack")
			//.Respond("application/x-msgpack", "��Key�valeera�Name�Valeera�Title�Shadow of the Uncrowned")
			.Respond("application/x-msgpack", request => request.Content.ReadAsStreamAsync().Result);

		var fluentHttpClientFactory = BuildContainer()
			.GetRequiredService<IFluentHttpClientFactory>();

		var clientBuilder = fluentHttpClientFactory.CreateBuilder("newtonsoft")
				.WithBaseUrl("https://sketch7.com")
				.UseLogging(new LoggerHttpMiddlewareOptions
				{
					//ShouldLogDetailedRequest = true,
					//ShouldLogDetailedResponse = true
				})
				.UseTimer()
				.WithMessageHandler(mockHttp)
			;

		_jsonHttpClient = clientBuilder
			.ConfigureFormatters(x => x.Default = x.Formatters.JsonFormatter)
			.Build();

		_messagePackHttpClient = clientBuilder.WithIdentifier("msgpacks")
				.ConfigureFormatters(x => x.Default = new MessagePackMediaTypeFormatter(ContractlessStandardResolver.Options))
				.Build()
			;

		_systemTextJsonHttpClient = clientBuilder.WithIdentifier("system.text.json")
				.ConfigureFormatters(x => x.Default = x.Formatters.SystemTextJsonFormatter())
				.Build()
			;

		Console.WriteLine($"Setup Complete");
		Console.WriteLine($" - _jsonHttpClient: {_jsonHttpClient.DefaultFormatter.GetType().Name}");
		Console.WriteLine($" - _messagePackHttpClient: {_messagePackHttpClient.DefaultFormatter.GetType().Name}");
		Console.WriteLine($" - _systemTextJsonHttpClient: {_systemTextJsonHttpClient.DefaultFormatter.GetType().Name}");
	}

	[Benchmark]
	public Task<Hero> PostAsJson()
	{
		return _jsonHttpClient.CreateRequest("/api/json")
			.AsPost()
			.WithBody(_request)
			.Return<Hero>();
	}

	[Benchmark]
	public Task<Hero> PostAsMessagePack()
	{
		return _messagePackHttpClient.CreateRequest("/api/msgpack")
			.AsPost()
			.WithBody(_request)
			.Return<Hero>();
	}

	[Benchmark]
	public Task<Hero> PostAsSystemTextJson()
	{
		return _systemTextJsonHttpClient.CreateRequest("/api/json")
			.AsPost()
			.WithBody(_request)
			.Return<Hero>();
	}
}