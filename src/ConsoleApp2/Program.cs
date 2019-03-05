using System;
using FluentHttpClient.Entity;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleApp2
{
	public class Program
	{
		public static void Main(string[] args)
		{
			Console.WriteLine("starting...");

			var serviceProvider = new ServiceCollection()
				.AddFluentHttpClientEntity()
				.BuildServiceProvider();

			var dbContext = serviceProvider.GetService<FluentHttpClientContext>();

			dbContext.Initialize();

			Console.WriteLine("DONE!!!");

		}
	}
}