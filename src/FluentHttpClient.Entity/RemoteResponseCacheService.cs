using System.Threading.Tasks;
using FluentlyHttpClient;
using FluentlyHttpClient.Caching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FluentHttpClient.Entity
{
	public class RemoteResponseCacheService : IResponseCacheService
	{
		private readonly FluentHttpClientContext _client;
		private readonly IHttpResponseSerializer _serializer;
		private readonly IMemoryCache _cache;

		public RemoteResponseCacheService(
			FluentHttpClientContext client,
			IHttpResponseSerializer serializer,
			IMemoryCache cache)
		{
			_client = client;
			_serializer = serializer;
			_cache = cache;
		}

		public async Task<FluentHttpResponse> Get(string hash)
		{
			var result = await _cache.GetOrCreate(hash, async _ =>
			{
				var item = await _client.HttpResponses.FirstOrDefaultAsync(x => x.Hash == hash);

				if (item == null) return null;

				return await _serializer.Deserialize(item);
			});

			return await result.Clone();
		}

		public async Task Set(string hash, FluentHttpResponse response)
		{
			var item = await _serializer.Serialize(response);
			await _client.HttpResponses.AddAsync(item);
			await _client.Commit();

			_cache.Set(hash, response);
		}
	}
}