using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;

namespace FluentlyHttpClient.Sample.Api.Controllers
{
	public class UploadInput
	{
		[Required]
		public string Hero { get; set; }

		[Required]
		public IFormFile File { get; set; }

	}


	[Route("api/[controller]")]
	[ApiController]
	public class SampleController : ControllerBase
	{
		//private readonly FluentHttpClientDbContext _dbContext;

		public SampleController(
		//FluentHttpClientDbContext dbContext
		)
		{
			//_dbContext = dbContext;
		}

		// GET api/values
		[HttpGet]
		public async Task<IEnumerable<string>> Get()
		{
			//await _dbContext.Initialize();
			return new string[] { "value1", "value2" };
		}

		// GET api/values/5
		[HttpGet("{id}")]
		public ActionResult<string> Get(int id)
		{
			return "value";
		}

		// POST api/values
		[HttpPost("upload")]
		public IActionResult Upload([FromForm] UploadInput input)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var file = input.File;
			if (file.Length > 0)
			{
				using (var fileStream = new FileStream(file.FileName, FileMode.Create))
				{
					file.CopyTo(fileStream);
				}
			}

			return Ok(new
			{
				input.Hero,
				file.FileName,
				file.ContentType,
				Size = file.Length.Bytes().Kilobytes
			});

		}

		// PUT api/values/5
		[HttpPut("{id}")]
		public void Put(int id, [FromBody] string value)
		{
		}

		// DELETE api/values/5
		[HttpDelete("{id}")]
		public void Delete(int id)
		{
		}
	}
}
