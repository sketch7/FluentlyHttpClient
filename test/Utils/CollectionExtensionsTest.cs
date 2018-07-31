using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace FluentlyHttpClient.Test.Utils
{
	public class CollectionExtensionsTest
	{
		[Fact]
		public void ShouldGenerateBasicQueryString()
		{
			var queryCollection = new Dictionary<string, IEnumerable>
			{
				{"heroName", "yasuo"},
				{"filter", "assassin"}
			};

			var result = queryCollection.ToQueryString();

			Assert.Equal("heroName=yasuo&filter=assassin", result);
		}

		[Fact]
		public void ShouldGenerateComplexQueryString()
		{
			var queryCollection = new Dictionary<string, IEnumerable>
			{
				{"heroName", "yasuo"},
				{"filter", new List<string>{ "assassin", "fighter" }}
			};

			var result = queryCollection.ToQueryString();

			Assert.Equal("heroName=yasuo&filter=assassin&filter=fighter", result);
		}

		[Fact]
		public void ShouldReturnEmptyString()
		{
			var queryCollection = new Dictionary<string, IEnumerable>();

			var result = queryCollection.ToQueryString();

			Assert.Empty(result);
		}
	}
}