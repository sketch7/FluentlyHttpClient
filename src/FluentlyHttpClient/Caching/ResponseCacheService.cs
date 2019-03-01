using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FluentlyHttpClient.Caching
{
	public interface IResponseCacheService
	{
		Task<FluentHttpResponse> Get(string hash, FluentHttpRequest request);
		Task Set(string hash, FluentHttpResponse response);
		bool Matcher(FluentHttpRequest request);
		string GenerateHash(FluentHttpRequest request);
	}

	public class MemoryResponseCacheService : IResponseCacheService
	{
		private readonly Dictionary<string, FluentHttpResponse> _cache = new Dictionary<string, FluentHttpResponse>();

		public async Task<FluentHttpResponse> Get(string hash, FluentHttpRequest request)
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

		// todo: make global configurable instead?
		public bool Matcher(FluentHttpRequest request)
		{
			return true;
		}

		// todo: make this more reusable/overridable e.g. IRequestHashGenerator?
		public string GenerateHash(FluentHttpRequest request)
		{
			var headers = request.Builder.DefaultHeaders.ToDictionary();
			foreach (var requestHeader in request.Headers)
				headers[requestHeader.Key] = string.Join(";", requestHeader.Value);

			var urlHash = request.Uri.IsAbsoluteUri
				? request.Uri
				: new Uri($"{request.Builder.BaseUrl.TrimEnd('/')}/{request.Uri.ToString().TrimStart('/')}");
			var headersHash = headers.ToHeadersHashString();

			var hash = $"method={request.Method};url={urlHash};headers={headersHash}";
			return hash;
		}

	}


}