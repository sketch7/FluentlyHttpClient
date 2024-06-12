namespace FluentlyHttpClient.Test.Utils;

public class QueryStringUtils_ToQueryString
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
	public void ShouldGenerateComplexQueryStringWithoutCollection()
	{
		var queryCollection = new Dictionary<string, object>
		{
			{"heroName", "yasuo"},
			{"filter", new List<string>{ "" }},
		};

		var result = queryCollection.ToQueryString();

		Assert.Equal("heroName=yasuo", result);
	}

	[Fact]
	public void ShouldOmitEmptyOrNulls()
	{
		var queryCollection = new Dictionary<string, object?>
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
	public void ShouldFormatCollectionsWithFormatter() // deprecated: remove
	{
		var queryCollection = new Dictionary<string, object>
		{
			{"heroName", "yasuo"},
			{"filter", new List<HeroRole>{ HeroRole.Assassin, HeroRole.Fighter }},
			{"heroType", HeroRole.Assassin},
		};

		var result = queryCollection.ToQueryString(opts =>
		{
			opts.CollectionMode = QueryStringCollectionMode.CommaSeparated;
#pragma warning disable 618
			opts.WithCollectionItemFormatter(valueObj =>
#pragma warning restore 618
			{
				if (valueObj is Enum @enum)
					return @enum.GetEnumDescription();
				return valueObj.ToString();
			});
		});

		Assert.Equal("heroName=yasuo&filter=assassin,fighter&heroType=Assassin", result);
	}

	[Fact]
	public void ShouldFormatValueWithFormatter()
	{
		var queryCollection = new Dictionary<string, object>
		{
			{"heroName", "yasuo"},
			{"filter", new List<HeroRole>{ HeroRole.Assassin, HeroRole.Fighter }},
			{"heroType", HeroRole.Assassin},
		};

		var result = queryCollection.ToQueryString(opts =>
		{
			opts.CollectionMode = QueryStringCollectionMode.CommaSeparated;
			opts.WithValueFormatter(valueObj =>
			{
				if (valueObj is Enum @enum)
					return @enum.GetEnumDescription();
				return valueObj.ToString();
			});
		});

		Assert.Equal("heroName=yasuo&filter=assassin,fighter&heroType=assassin", result);
	}

	[Fact]
	public void ShouldFormatKeyToLower()
	{
		var queryCollection = new Dictionary<string, object>
		{
			{"HeroName", "yasuo"},
			{"Level", 100}
		};

		var result = queryCollection.ToQueryString(opts => opts.WithKeyFormatter(key => key.ToLower()));

		Assert.Equal("heroname=yasuo&level=100", result);
	}

	[Fact]
	public void ShouldFormatCollectionKey()
	{
		var queryCollection = new Dictionary<string, object>
		{
			{"HeroName", "yasuo"},
			{"filter", new List<string>{ "assassin", "fighter" }}
		};

		var result = queryCollection.ToQueryString(opts =>
			{
				opts.CollectionMode = QueryStringCollectionMode.KeyPerValue;
				opts.WithCollectionKeyFormatter(key => $"{key}[]");
			}
		);

		Assert.Equal("heroName=yasuo&filter[]=assassin&filter[]=fighter", result);
	}

	[Fact]
	public void ShouldNotHttpEncodeValue()
	{
		var queryCollection = new Dictionary<string, object>
		{
			{"HeroName", "yas,uo"},
			{"Level", 100}
		};

		var result = queryCollection.ToQueryString(opts => opts.WithValueEncoder(value => value));

		Assert.Equal("heroName=yas,uo&level=100", result);
	}

	[Fact]
	public void ShouldHttpEncodeValue()
	{
		var queryCollection = new Dictionary<string, object>
		{
			{"HeroName", "yas,uo"},
			{"Level", 100}
		};

		var result = queryCollection.ToQueryString();

		Assert.Equal("heroName=yas%2cuo&level=100", result);
	}

}