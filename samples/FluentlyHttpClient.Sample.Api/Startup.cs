using FluentlyHttpClient.Sample.Api.Heroes;
using MessagePack.AspNetCoreMvcFormatter;
using MessagePack.Resolvers;

namespace FluentlyHttpClient.Sample.Api;

public class Startup
{
	public Startup(IConfiguration configuration)
	{
		Configuration = configuration;
	}

	public IConfiguration Configuration { get; }

	// This method gets called by the runtime. Use this method to add services to the container.
	public void ConfigureServices(IServiceCollection services)
	{
		services
			.AddSingleton<IHeroService, HeroService>()
			.AddFluentlyHttpClient()
			//.AddFluentlyHttpClientEntity(Configuration.GetConnectionString("FluentlyDatabase"))
			.AddControllers(opts =>
				{
					opts.OutputFormatters.Add(new MessagePackOutputFormatter(ContractlessStandardResolver.Options));
					opts.InputFormatters.Add(new MessagePackInputFormatter(ContractlessStandardResolver.Options));
				}
			)
			;
	}

	// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
	public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
	{
		if (env.IsDevelopment())
		{
			app.UseDeveloperExceptionPage();
		}

		app.UseRouting();

		app.UseAuthorization();

		app.UseEndpoints(endpoints =>
		{
			endpoints.MapControllers();
		});
	}
}