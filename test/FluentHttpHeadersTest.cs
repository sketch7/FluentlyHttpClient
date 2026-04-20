using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

// ReSharper disable InconsistentNaming
namespace FluentlyHttpClient.Test;

public class FluentHttpHeaders_Tests
{
	[Fact]
	public void ToDictionary_ShouldBeConverted()
	{
		var headers = new FluentHttpHeaders
		{
			{HeaderTypes.Accept, ["json", "msgpack"] }
		};

		var dictionary = headers.ToDictionary();

		var result = dictionary.GetValueOrDefault(HeaderTypes.Accept);
		result.ShouldNotBeNull();
		result.Length.ShouldBe(2);
		result[0].ShouldBe("json");
		result[1].ShouldBe("msgpack");
	}

	[Fact]
	public void ShouldBeSerializable()
	{
		var headers = new FluentHttpHeaders
		{
			{HeaderTypes.Authorization, ["the-xx"] },
			{HeaderTypes.Accept, ["json", "msgpack"] },
			{HeaderTypes.XForwardedHost, ["sketch7.com"] },
		};

		var headersJson = JsonConvert.SerializeObject(headers);
		var headersCopied = JsonConvert.DeserializeObject<FluentHttpHeaders>(headersJson);

		headersCopied.ShouldNotBeNull();
		headersCopied.Count.ShouldBe(3);
		var items = headersCopied.ToList();
		items[0].Key.ShouldBe(HeaderTypes.Authorization);
		items[0].Value![0].ShouldBe("the-xx");
		items[1].Key.ShouldBe(HeaderTypes.Accept);
		string.Join(",", items[1].Value!).ShouldBe("json,msgpack");
		items[2].Key.ShouldBe(HeaderTypes.XForwardedHost);
		items[2].Value![0].ShouldBe("sketch7.com");
		headersCopied.Authorization.ShouldBe("the-xx");
	}

	[Fact]
	public void HeaderBuilderExtMethod_ShouldBeAvailable()
	{
		var headers = new FluentHttpHeaders()
			.WithUserAgent("leoric");
		headers.UserAgent.ShouldBe("leoric");
	}
}

public class FluentHttpHeaders_Add
{
	[Fact]
	public void ShouldAdd()
	{
		var headers = new FluentHttpHeaders { { HeaderTypes.Authorization, "the-xx" } };

		headers.Authorization.ShouldBe("the-xx");
	}

	[Fact]
	public void ShouldAddWithStringValues()
	{
		var headers = new FluentHttpHeaders();

		StringValues str = new[] { "the-xx", "supersecret" };
		headers.Add(HeaderTypes.Authorization, str);
		headers.Authorization.ShouldBe("the-xx,supersecret");
	}

	[Fact]
	public void ShouldAddEnumerable()
	{
		var headers = new FluentHttpHeaders
		{
			{ HeaderTypes.Accept, ["json", "msgpack"] }
		};
		((string?)headers.Accept).ShouldBe("json,msgpack");
	}

	[Fact]
	public void AddRangeSame_ShouldThrow()
	{
		var headers = new FluentHttpHeaders();
		headers.AddRange(new Dictionary<string, StringValues>{
			{HeaderTypes.Accept, new[] {"json", "msgpack"}}
		});
		Should.Throw<ArgumentException>(() => headers.AddRange(new Dictionary<string, StringValues>{
			{HeaderTypes.Accept, "xml"}
		}));
	}

	[Fact]
	public void SetRangeSame_ShouldUpdateWithLatest()
	{
		var headers = new FluentHttpHeaders();
		headers.SetRange(new Dictionary<string, StringValues>{
			{HeaderTypes.Accept, new[] {"json", "msgpack"}}
		});
		headers.SetRange(new Dictionary<string, StringValues>{
			{HeaderTypes.Accept, "xml"}
		});
		((string?)headers.Accept).ShouldBe("xml");
	}
}

public class FluentHttpHeaders_Remove
{
	[Fact]
	public void ShouldRemoveExisting()
	{
		var headers = new FluentHttpHeaders
		{
			{ HeaderTypes.Authorization, "the-xx" }
		};
		headers.Remove(HeaderTypes.Authorization);
		headers.Authorization.ShouldBeNull();
	}

	[Fact]
	public void ShouldNotThrowWhenRemovingNonExisting()
	{
		var headers = new FluentHttpHeaders();
		headers.Remove(HeaderTypes.Authorization);
		headers.Authorization.ShouldBeNull();
	}
}

public class FluentHttpHeaders_GetValue
{
	[Fact]
	public void ShouldBeCaseInsensitive()
	{
		var headers = new FluentHttpHeaders
		{
			{ "X-Custom", "the-xx" }
		};
		var value = headers.GetValue("X-Custom");
		var value2 = headers.GetValue("x-custom");
		value.ShouldBe("the-xx");
		value2.ShouldBe("the-xx");
	}
}

public class FluentHttpHeaders_Accessors
{
	[Fact]
	public void GetNotExists_ShouldReturnNull()
	{
		var headers = new FluentHttpHeaders();
		headers.UserAgent.ShouldBeNull();
	}

	[Fact]
	public void GetExists_ShouldReturn()
	{
		var headers = new FluentHttpHeaders()
			.Add(HeaderTypes.Accept, ["json", "msgpack"]);
		((string?)headers.Accept).ShouldBe("json,msgpack");
	}

	[Fact]
	public void SetNotExists_ShouldBeAdded()
	{
		var headers = new FluentHttpHeaders
		{
			Accept = new[] { "json", "msgpack" }
		};
		((string?)headers.Accept).ShouldBe("json,msgpack");
	}

	[Fact]
	public void SetExists_ShouldBeUpdated()
	{
		var headers = new FluentHttpHeaders
		{
			Accept = new[] { "json", "msgpack" }
		};
		headers.Accept = "json";
		((string?)headers.Accept).ShouldBe("json");
	}
}

public class FluentHttpHeaders_Initialize
{
	[Fact]
	public void ShouldInitializeFromDictionaryOfString()
	{
		var headersMap = new Dictionary<string, string>
		{
			{HeaderTypes.Authorization, "the-xx"},
			{HeaderTypes.ContentType, "json" }
		};

		var headers = new FluentHttpHeaders(headersMap);
		headers.Authorization.ShouldBe("the-xx");
		headers.ContentType.ShouldBe("json");
		headers.Count.ShouldBe(headersMap.Count);
	}

	[Fact]
	public void ShouldInitializeFromDictionaryOfStringValues()
	{
		var headersMap = new Dictionary<string, StringValues>
		{
			{HeaderTypes.Authorization, "the-xx"},
			{HeaderTypes.ContentType, "json" }
		};

		var headers = new FluentHttpHeaders(headersMap);
		headers.Authorization.ShouldBe("the-xx");
		headers.ContentType.ShouldBe("json");
		headers.Count.ShouldBe(headersMap.Count);
	}

	[Fact]
	public void ShouldInitializeFromDictionaryOfEnumerableString()
	{
		var headersMap = new Dictionary<string, IEnumerable<string>>
		{
			{HeaderTypes.Accept, ["json", "msgpack"] },
			{HeaderTypes.XForwardedFor, ["192.168.1.1", "127.0.0.1"] },
		};

		var headers = new FluentHttpHeaders(headersMap);
		((string?)headers.Accept).ShouldBe("json,msgpack");
		((string?)headers.XForwardedFor).ShouldBe("192.168.1.1,127.0.0.1");
		headers.Count.ShouldBe(headersMap.Count);
	}

	[Fact]
	public void ShouldInitializeFromHttpHeaders()
	{
		var httpHeaders = new HttpRequestMessage().Headers;
		httpHeaders.Add(HeaderTypes.Authorization, "the-xx");
		httpHeaders.Add(HeaderTypes.XForwardedFor, ["192.168.1.1", "127.0.0.1"]);

		var headers = new FluentHttpHeaders(httpHeaders);
		headers.Authorization.ShouldBe("the-xx");
		((string?)headers.XForwardedFor).ShouldBe("192.168.1.1,127.0.0.1");
		headers.Count.ShouldBe(2);
	}
}

public class FluentHttpHeaders_ToHashString
{
	[Fact]
	public void ShouldHashSimple()
	{
		var headers = new FluentHttpHeaders
		{
			{HeaderTypes.Authorization, "the-xx"},
			{HeaderTypes.ContentType,"json" }
		};
		var hash = headers.ToHashString();

		hash.ShouldBe("Authorization=the-xx&Content-Type=json");
	}

	[Fact]
	public void ShouldHashWithEnumerable()
	{
		var headers = new FluentHttpHeaders
		{
			{HeaderTypes.Authorization, "the-xx"},
			{HeaderTypes.Accept, ["json", "msgpack"] }
		};
		var hash = headers.ToHashString();

		hash.ShouldBe("Authorization=the-xx&Accept=json,msgpack");
	}

	[Fact]
	public void ShouldFilterWithHashingFilter()
	{
		var headers = new FluentHttpHeaders
			{
				{HeaderTypes.Authorization, "the-xx"},
				{HeaderTypes.Accept, ["json", "msgpack"] },
				{HeaderTypes.XForwardedHost, "sketch7.com"},
			}
			.WithOptions(opts => opts.WithHashingExclude(pair => pair.Key == HeaderTypes.Authorization));
		var hash = headers.ToHashString();

		hash.ShouldBe("Accept=json,msgpack&X-Forwarded-Host=sketch7.com");
	}
}