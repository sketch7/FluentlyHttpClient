using System;
using System.Collections.Generic;
using Xunit;

namespace FluentlyHttpClient.Test.Utils
{
	public class CollectionExt_ToQueryString
	{
		[Fact]
		public void ShouldGenerateBasicQueryString()
		{
			var queryCollection = new Dictionary<string, object>
				{
					{"heroName", "yasuo"},
					{"filter", "assassin"}
				};

			var result = queryCollection.ToQueryString();

			Assert.Equal("heroName=yasuo&filter=assassin", result);
		}

		[Fact]
		public void ShouldGenerateComplexQueryStringWithCollection()
		{
			var queryCollection = new Dictionary<string, object>
				{
					{"heroName", "yasuo"},
					{"filter", new List<string>{ "assassin", "fighter" }}
				};

			var result = queryCollection.ToQueryString();

			Assert.Equal("heroName=yasuo&filter=assassin&filter=fighter", result);
		}


		[Fact]
		public void ShouldOmitEmptyOrNulls()
		{
			var queryCollection = new Dictionary<string, object>
				{
					{"heroName", ""},
					{"cap", null},
					{"filter", new List<string>{ }},
					{"level", 100},
				};

			var result = queryCollection.ToQueryString();

			Assert.Equal("level=100", result);
		}

		[Fact]
		public void ShouldReturnEmptyString()
		{
			var queryCollection = new Dictionary<string, object>();

			var result = queryCollection.ToQueryString();

			Assert.Empty(result);
		}

		[Fact]
		public void ShouldFormatCollectionsCommaSeparated()
		{
			var queryCollection = new Dictionary<string, object>
			{
				{"heroName", "yasuo"},
				{"filter", new List<string>{ "assassin", "fighter" }}
			};
			var result = queryCollection.ToQueryString(opts => opts.CollectionMode = QueryStringCollectionMode.CommaSeparated);

			Assert.Equal("heroName=yasuo&filter=assassin,fighter", result);
		}

		[Fact]
		public void ShouldFormatCollectionsWithFormatter()
		{
			var queryCollection = new Dictionary<string, object>
			{
				{"heroName", "yasuo"},
				{"filter", new List<HeroRole>{ HeroRole.Assassin, HeroRole.Fighter }}
			};

			var result = queryCollection.ToQueryString(opts =>
			{
				opts.CollectionMode = QueryStringCollectionMode.CommaSeparated;
				opts.CollectionItemFormatter = valueObj =>
				{
					if (valueObj is Enum @enum)
						return @enum.GetEnumDescription();
					return valueObj.ToString();
				};
			});

			Assert.Equal("heroName=yasuo&filter=assassin,fighter", result);
		}

		[Fact]
		public void ShouldFormatKeyToLower()
		{
			var queryCollection = new Dictionary<string, object>
			{
				{"HeroName", "yasuo"},
				{"Level", 100}
			};

			var result = queryCollection.ToQueryString(opts => opts.KeyFormatter = key => key.ToLower());

			Assert.Equal("heroname=yasuo&level=100", result);
		}

	}
}