
using FluentlyHttpClient.Sample.Api.Heroes;
using Humanizer;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost
	.UseUrls("http://localhost:5500/", "https://localhost:5510")
	.ConfigureKestrel(opts => opts
		.ConfigureEndpointDefaults(lo => lo.Protocols = HttpProtocols.Http1AndHttp2AndHttp3)
	);

builder.Services
	.AddSingleton<IHeroService, HeroService>()
	.AddFluentlyHttpClient()
	.AddEndpointsApiExplorer();

var app = builder.Build();

if (app.Environment.IsDevelopment())
	app.UseDeveloperExceptionPage();

// Heroes endpoints
var heroes = app.MapGroup("/api/heroes");

heroes.MapGet("/", async (IHeroService service)
	=> Results.Ok(await service.GetAll()));

heroes.MapGet("/{key}", async (string key, IHeroService service) =>
{
	var hero = await service.GetByKey(key);
	return hero is null ? Results.NotFound() : Results.Ok(hero);
});

heroes.MapPost("/", async (Hero input, IHeroService service) =>
{
	await service.Add(input);
	return Results.Ok(input);
});

// Sample endpoints
var sample = app.MapGroup("/api/sample");

sample.MapGet("/", () => Results.Ok(new[] { "value1", "value2" }));

sample.MapGet("/{id:int}", (int id) => Results.Ok("value"));

sample.MapPost("/upload", async (IFormFile file, [Microsoft.AspNetCore.Mvc.FromForm] string hero) =>
	Results.Ok(new
	{
		Hero = hero,
		file.FileName,
		file.ContentType,
		Size = file.Length.Bytes().Kilobytes
	})).DisableAntiforgery();

sample.MapPut("/{id:int}", (int id, string value) => Results.NoContent());

sample.MapDelete("/{id:int}", (int id) => Results.NoContent());

app.Run();

/// <summary>Partial class entry point — required for WebApplicationFactory in integration tests.</summary>
public partial class Program { }