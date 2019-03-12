using System;
using BenchmarkDotNet.Running;

namespace FluentlyHttpClient.Benchmarks
{
	static class Program
	{
		static void Main(string[] args)
		{
			//var benchmarking = new Benchmarking();
			//benchmarking.Setup();

			//benchmarking.PostAsMessagePack().Wait();
			//benchmarking.PostAsJson().Wait();
			var summary = BenchmarkRunner.Run<Benchmarking>();

		}
	}
}
