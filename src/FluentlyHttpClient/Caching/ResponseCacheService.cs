using System.Collections.Generic;
using System.Threading.Tasks;

namespace FluentlyHttpClient.Caching
{
	public interface IResponseCacheService
	{
		Task<FluentHttpResponse> Get(string hash);
		Task Set(string hash, FluentHttpResponse response);
	}

	public class MemoryResponseCacheService : IResponseCacheService
	{
		private readonly Dictionary<string, FluentHttpResponse> _cache = new Dictionary<string, FluentHttpResponse>();

		public async Task<FluentHttpResponse> Get(string hash)
		{
			_cache.TryGetValue(hash, out var response);
			if (response == null)
				return null;

			var cloned = await response.Clone();
			return cloned;
		}

		public async Task Set(string hash, FluentHttpResponse response)
		{
			var cloned = await response.Clone();
			_cache[hash] = cloned;
		}

	}

}