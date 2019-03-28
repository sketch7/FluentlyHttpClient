using System.Threading.Tasks;
using FluentlyHttpClient.Caching;
using Microsoft.Extensions.Caching.Memory;
using Sketch7.Core;

namespace FluentlyHttpClient.Entity
{
	public class RemoteResponseCacheService : IResponseCacheService
	{
		private readonly FluentHttpClientDbContext _dbContext;
		private readonly IHttpResponseSerializer _serializer;
		private readonly IMemoryCache _cache;

		public RemoteResponseCacheService(
			FluentHttpClientDbContext dbContext,
			IHttpResponseSerializer serializer,
			IMemoryCache cache
		)
		{
			_dbContext = dbContext;
			_serializer = serializer;
			_cache = cache;
		}

		public async Task<FluentHttpResponse> Get(string hash)
		{
			var id = await hash.ComputeHash();
			var result = await _cache.GetOrCreate(id, async _ =>
			{
				var item = await _dbContext.HttpResponses.FindAsync(id);

				if (item == null) return null;

				return await _serializer.Deserialize(item);
			});

			return result != null ? await result.Clone() : null;
		}

		public async Task Set(string hash, FluentHttpResponse response)
		{
			var item = await _serializer.Serialize<HttpResponseEntity>(response);
			item.Id = await hash.ComputeHash();

			await _dbContext.HttpResponses.AddAsync(item);
			await _dbContext.Commit();

			await _cache.Set(item.Id, Task.FromResult(response));
		}
	}
}