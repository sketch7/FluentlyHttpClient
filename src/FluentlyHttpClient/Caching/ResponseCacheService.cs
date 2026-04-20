using Microsoft.Extensions.Caching.Memory;

namespace FluentlyHttpClient.Caching;

/// <summary>Contract for caching HTTP responses by request hash.</summary>
public interface IResponseCacheService
{
	/// <summary>Get a cached response by hash, or <see langword="null"/> if not found.</summary>
	Task<FluentHttpResponse?> Get(string hash);

	/// <summary>Store a response under the given hash.</summary>
	Task Set(string hash, FluentHttpResponse response);
}

/// <summary>In-memory implementation of <see cref="IResponseCacheService"/> backed by <see cref="IMemoryCache"/>.</summary>
public class MemoryResponseCacheService : IResponseCacheService
{
	private readonly IMemoryCache _cache;

	/// <summary>Initializes a new instance of <see cref="MemoryResponseCacheService"/>.</summary>
	/// <param name="cache">The memory cache to use.</param>
	public MemoryResponseCacheService(IMemoryCache cache)
	{
		_cache = cache;
	}

	/// <inheritdoc />
	public async Task<FluentHttpResponse?> Get(string hash)
	{
		var result = _cache.Get<FluentHttpResponse>(hash);
		return result != null ? await result.Clone() : null;
	}

	/// <inheritdoc />
	public Task Set(string hash, FluentHttpResponse response)
	{
		_cache.Set(hash, response);
		return Task.CompletedTask;
	}
}