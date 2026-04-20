using FluentlyHttpClient.Caching;
using Microsoft.Extensions.Caching.Memory;
using Sketch7.Core;

namespace FluentlyHttpClient.Entity;

/// <summary>SQL Server-backed implementation of <see cref="IResponseCacheService"/> using EF Core with a local memory cache layer.</summary>
public class RemoteResponseCacheService : IResponseCacheService
{
	private readonly FluentHttpClientDbContext _dbContext;
	private readonly IHttpResponseSerializer _serializer;
	private readonly IMemoryCache _cache;

	/// <summary>Initializes a new instance of <see cref="RemoteResponseCacheService"/>.</summary>
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

	/// <inheritdoc />
	public async Task<FluentHttpResponse?> Get(string hash)
	{
		var id = await hash.ComputeHash();
		var cachedTask = _cache.GetOrCreate(id, async _ =>
		{
			var item = await _dbContext.HttpResponses.FindAsync(id);

			if (item == null) return null;

			return await _serializer.Deserialize(item);
		});
		var result = cachedTask != null ? await cachedTask : null;
		return result != null ? await result.Clone() : null;
	}

	/// <inheritdoc />
	public async Task Set(string hash, FluentHttpResponse response)
	{
		var item = await _serializer.Serialize<HttpResponseEntity>(response);
		item.Id = await hash.ComputeHash();

		await _dbContext.HttpResponses.AddAsync(item);
		await _dbContext.Commit();

		await _cache.Set(item.Id, Task.FromResult(response));
	}
}