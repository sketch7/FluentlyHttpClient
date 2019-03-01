using FluentlyHttpClient.Caching;
using Xunit;

namespace FluentlyHttpClient.Test
{
	public class MemoryResponseCache_GenerateHash
	{
		private static IFluentHttpClient GetClient()
		{
			var fluentHttpClientFactory = ServiceTestUtil.GetNewClientFactory();
			var clientBuilder = fluentHttpClientFactory.CreateBuilder("sketch7")
					//.WithBaseUrl("https://localhost:5001")
					.WithBaseUrl("http://local.sketch7.io:5000")
					.WithHeader("locale", "en-GB")
					.WithHeader("X-SSV-VERSION", "2019.02-2")
				;
			return fluentHttpClientFactory.Add(clientBuilder);
		}

		[Fact]
		public void ShouldBeEqual()
		{
			var service = new MemoryResponseCacheService();
			var httpClient = GetClient();

			var request1 = httpClient.CreateRequest("/api/heroes/azmodan");
			var request2 = httpClient.CreateRequest("/api/heroes/azmodan");

			var request1Hash = service.GenerateHash(request1.Build());
			var request2Hash = service.GenerateHash(request2.Build());

			Assert.Equal(request1Hash, request2Hash);
		}

		[Fact]
		public void ShouldHandleHeaders()
		{
			var service = new MemoryResponseCacheService();
			var httpClient = GetClient();

			var requestWithToken = httpClient.CreateRequest("/api/heroes/azmodan")
				.WithBearerAuthentication("XXX");
			var noTokenRequest = httpClient.CreateRequest("/api/heroes/azmodan");

			var tokenRequestHash = service.GenerateHash(requestWithToken.Build());
			var noTokenRequestHash = service.GenerateHash(noTokenRequest.Build());

			Assert.NotEqual(tokenRequestHash, noTokenRequestHash);
		}

	}
}
