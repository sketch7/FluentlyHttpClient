using FluentlyHttpClient.Sample.Api.Heroes;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace FluentlyHttpClient.Sample.Api.Controllers;

// GET
[Route("api/[controller]")]
[ApiController]
public class HeroesController(
	IHeroService service
) : Controller
{
	// GET api/heroes
	[HttpGet]
	public async Task<IEnumerable<Hero>> Get()
	{
		return await service.GetAll();
	}

	// GET api/heroes/azmodan
	[HttpGet("{key}")]
	public async Task<IActionResult> Get(string key)
	{
		var hero = await service.GetByKey(key);
		if (hero == null)
			return NotFound();
		return Ok(hero);
	}

	// PUT api/heroes/azmodan
	[HttpPost]
	public async Task<IActionResult> Post([FromBody][Required] Hero input)
	{
		if (!ModelState.IsValid)
			return BadRequest(ModelState);

		//try
		//{
		await service.Add(input);
		//}
		//catch (ApiException ex)
		//{
		//	return BadRequest(new
		//	{
		//		ex.ErrorCode
		//	});
		//}
		return Ok(input);
	}
}
