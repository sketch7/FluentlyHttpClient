using System.Threading.Tasks;
using FluentHttpClient.Entity.Extensions;
using FluentlyHttpClient;
using FluentlyHttpClient.Caching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FluentHttpClient.Entity
{
	public class RemoteResponseCacheService : IResponseCacheService
	{
		private readonly FluentHttpClientDbContext _clientDb;
		private readonly IHttpResponseSerializer _serializer;
		private readonly IMemoryCache _cache;

		public RemoteResponseCacheService(
			FluentHttpClientDbContext clientDb,
			IHttpResponseSerializer serializer,
			IMemoryCache cache)
		{
			_clientDb = clientDb;
			_serializer = serializer;
			_cache = cache;
		}

		public async Task<FluentHttpResponse> Get(string hash)
		{
			var id = await hash.ComputeHash();
			var result = await _cache.GetOrCreate(id, async _ =>
			{
				var item = await _clientDb.HttpResponses.FirstOrDefaultAsync(x => x.Id == id);

				if (item == null) return null;

				return await _serializer.Deserialize(item);
			});

			return result != null ? await result.Clone() : null;
		}

		public async Task Set(string hash, FluentHttpResponse response)
		{
			var item = await _serializer.Serialize<HttpResponse>(response);
			item.Id = await hash.ComputeHash();

			await _clientDb.HttpResponses.AddAsync(item);
			await _clientDb.Commit();

			await _cache.Set(hash, Task.FromResult(response));
		}
	}
}