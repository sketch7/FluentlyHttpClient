using BenchmarkDotNet.Running;
using FluentlyHttpClient.Benchmarks;

var summary = BenchmarkRunner.Run<Benchmarking>();