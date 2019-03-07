using System.Threading.Tasks;
using FluentlyHttpClient;
using FluentlyHttpClient.Caching;
using Microsoft.EntityFrameworkCore;

namespace FluentHttpClient.Entity
{
	public class RemoteResponseCacheService : IResponseCacheService
	{
		private readonly FluentHttpClientContext _client;
		private readonly IHttpResponseSerializer _serializer;

		public RemoteResponseCacheService(FluentHttpClientContext client, IHttpResponseSerializer serializer)
		{
			_client = client;
			_serializer = serializer;
		}

		public async Task<FluentHttpResponse> Get(string hash)
		{
			var item = await _client.HttpResponses.FirstOrDefaultAsync(x => x.Hash == hash);

			if (item == null) return null;

			var result = await _serializer.Deserialize(item);
			return result;
		}

		public async Task Set(string hash, FluentHttpResponse response)
		{
			var item = await _serializer.Serialize(response);
			await _client.HttpResponses.AddAsync(item);
			await _client.Commit();
		}
	}
}