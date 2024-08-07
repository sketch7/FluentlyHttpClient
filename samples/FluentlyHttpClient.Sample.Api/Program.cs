
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace FluentlyHttpClient.Sample.Api;

public class Program
{
	public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

	public static IHostBuilder CreateHostBuilder(string[] args)
		=> Host.CreateDefaultBuilder(args)
			.ConfigureWebHostDefaults(webBuilder =>
			{
				webBuilder
					.UseUrls("http://localhost:5500/", "https://localhost:5510")
					.ConfigureKestrel(opts => opts
						.ConfigureEndpointDefaults(lo => lo.Protocols = HttpProtocols.Http1AndHttp2AndHttp3)
					)
					.UseStartup<Startup>()
					;
			});
}