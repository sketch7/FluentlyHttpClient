using System;
using System.Net.Http;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using FluentlyHttpClient.Middleware;
using FluentlyHttpClient.Test;
using MessagePack.Resolvers;
using Microsoft.Extensions.DependencyInjection;
using RichardSzalay.MockHttp;
using Serilog;
using Sketch7.MessagePack.MediaTypeFormatter;

namespace FluentlyHttpClient.Benchmarks
{
	[ClrJob(baseline: true), CoreJob, MonoJob]
	[RPlotExporter, RankColumn]
	[MemoryDiagnoser]
	public class Benchmarking
	{
		private IFluentHttpClient _jsonHttpClient;
		private IFluentHttpClient _messagePackHttpClient;

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

			var clientBuilder = fluentHttpClientFactory.CreateBuilder("sketch7")
				.WithBaseUrl("https://sketch7.com")
				.UseLogging(new LoggerHttpMiddlewareOptions
				{
					//ShouldLogDetailedRequest = true,
					//ShouldLogDetailedResponse = true
				})
				.UseTimer()
				.WithMessageHandler(mockHttp);

			_jsonHttpClient = fluentHttpClientFactory.Add(clientBuilder);

			clientBuilder = fluentHttpClientFactory.CreateBuilder("msgpacks")
				.WithBaseUrl("https://sketch7.com")
				.UseLogging(new LoggerHttpMiddlewareOptions
				{
					//ShouldLogDetailedRequest = true,
					//ShouldLogDetailedResponse = true
				})
				.UseTimer()
				.WithMessageHandler(mockHttp)
				.ConfigureFormatters(x => x.Default = new MessagePackMediaTypeFormatter(ContractlessStandardResolver.Instance))
				;
			_messagePackHttpClient = fluentHttpClientFactory.Add(clientBuilder);
		}

		[Benchmark]
		public Task<Hero> PostAsJson()
		{
			return _jsonHttpClient.CreateRequest("/api/json")
				.AsPost()
				.WithBody(new Hero
				{
					Key = "valeera",
					Name = "Valeera",
					Title = "Shadow of the Uncrowned"
				})
				.Return<Hero>();
		}

		[Benchmark]
		public Task<Hero> PostAsMessagePack()
		{
			return _messagePackHttpClient.CreateRequest("/api/msgpack")
				.AsPost()
				.WithBody(new Hero
				{
					Key = "valeera",
					Name = "Valeera",
					Title = "Shadow of the Uncrowned"
				})
				.Return<Hero>();
		}


	}
}
