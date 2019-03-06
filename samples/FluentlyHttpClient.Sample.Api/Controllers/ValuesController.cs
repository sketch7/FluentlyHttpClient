using System.Collections.Generic;
using System.Threading.Tasks;
using FluentHttpClient.Entity;
using Microsoft.AspNetCore.Mvc;

namespace FluentlyHttpClient.Sample.Api.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ValuesController : ControllerBase
	{
		private readonly FluentHttpClientContext _client;
		private readonly IRemoteResponseCacheService _service;

		public ValuesController(FluentHttpClientContext client, IRemoteResponseCacheService service)
		{
			_client = client;
			_service = service;
		}

		// GET api/values
		[HttpGet]
		public async Task<IEnumerable<string>> Get()
		{
			await _client.Initialize();
			return new string[] { "value1", "value2" };
		}

		// GET api/values/5
		[HttpGet("{id}")]
		public ActionResult<string> Get(int id)
		{
			return "value";
		}

		// POST api/values
		[HttpPost]
		public void Post([FromBody] string value)
		{
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
