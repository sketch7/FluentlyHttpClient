using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace FluentlyHttpClient.Caching
{
	public interface IResponseCacheService
	{
		Task<FluentHttpResponse> Get(string hash);
		Task Set(string hash, FluentHttpResponse response);
	}

	public class MemoryResponseCacheService : IResponseCacheService
	{
		private readonly IMemoryCache _cache;

		public MemoryResponseCacheService(IMemoryCache cache)
		{
			_cache = cache;
		}

		public Task<FluentHttpResponse> Get(string hash)
		{
			var result = _cache.Get<FluentHttpResponse>(hash);
			return result?.Clone() ?? Task.FromResult<FluentHttpResponse>(null);
		}

		public Task Set(string hash, FluentHttpResponse response)
		{
			_cache.Set(hash, response);
			return Task.CompletedTask;
		}

	}

}