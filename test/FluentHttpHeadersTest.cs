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
			{HeaderTypes.Accept, new[] {"json", "msgpack"}}
		};

		var dictionary = headers.ToDictionary();

		var result = dictionary.GetValueOrDefault(HeaderTypes.Accept);
		Assert.Equal(2, result.Length);
		Assert.Equal("json", result[0]);
		Assert.Equal("msgpack", result[1]);
	}

	[Fact]
	public void ShouldBeSerializable()
	{
		var headers = new FluentHttpHeaders
		{
			{HeaderTypes.Authorization, new[]{"the-xx"}},
			{HeaderTypes.Accept, new[] {"json", "msgpack"}},
			{HeaderTypes.XForwardedHost, new[] {"sketch7.com"}},
		};

		var headersJson = JsonConvert.SerializeObject(headers);
		var headersCopied = JsonConvert.DeserializeObject<FluentHttpHeaders>(headersJson);

		Assert.Collection(headersCopied, x =>
			{
				Assert.Equal(HeaderTypes.Authorization, x.Key);
				Assert.Equal("the-xx", x.Value[0]);
			},
			x =>
			{
				Assert.Equal(HeaderTypes.Accept, x.Key);
				Assert.Equal("json,msgpack", string.Join(",", x.Value));
			},
			x =>
			{
				Assert.Equal(HeaderTypes.XForwardedHost, x.Key);
				Assert.Equal("sketch7.com", x.Value[0]);
			});
		Assert.Equal("the-xx", headersCopied.Authorization);
	}

	[Fact]
	public void HeaderBuilderExtMethod_ShouldBeAvailable()
	{
		var headers = new FluentHttpHeaders()
			.WithUserAgent("leoric");
		Assert.Equal("leoric", headers.UserAgent);
	}
}

public class FluentHttpHeaders_Add
{
	[Fact]
	public void ShouldAdd()
	{
		var headers = new FluentHttpHeaders();

		headers.Add(HeaderTypes.Authorization, "the-xx");
		Assert.Equal("the-xx", headers.Authorization);
	}

	[Fact]
	public void ShouldAddWithStringValues()
	{
		var headers = new FluentHttpHeaders();

		StringValues str = new[] { "the-xx", "supersecret" };
		headers.Add(HeaderTypes.Authorization, str);
		Assert.Equal("the-xx,supersecret", headers.Authorization);
	}

	[Fact]
	public void ShouldAddEnumerable()
	{
		var headers = new FluentHttpHeaders
		{
			{ HeaderTypes.Accept, new[] { "json", "msgpack" } }
		};
		Assert.Equal("json,msgpack", headers.Accept);
	}

	[Fact]
	public void AddRangeSame_ShouldThrow()
	{
		var headers = new FluentHttpHeaders();
		headers.AddRange(new Dictionary<string, StringValues>{
			{HeaderTypes.Accept, new[] {"json", "msgpack"}}
		});
		Assert.Throws<ArgumentException>(() => headers.AddRange(new Dictionary<string, StringValues>{
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
		Assert.Equal("xml", headers.Accept);
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
		Assert.Null(headers.Authorization);
	}

	[Fact]
	public void ShouldNotThrowWhenRemovingNonExisting()
	{
		var headers = new FluentHttpHeaders();
		headers.Remove(HeaderTypes.Authorization);
		Assert.Null(headers.Authorization);
	}
}

public class FluentHttpHeaders_Accessors
{
	[Fact]
	public void GetNotExists_ShouldReturnNull()
	{
		var headers = new FluentHttpHeaders();
		Assert.Null(headers.UserAgent);
	}

	[Fact]
	public void GetExists_ShouldReturn()
	{
		var headers = new FluentHttpHeaders()
			.Add(HeaderTypes.Accept, new[] { "json", "msgpack" });
		Assert.Equal("json,msgpack", headers.Accept);
	}

	[Fact]
	public void SetNotExists_ShouldBeAdded()
	{
		var headers = new FluentHttpHeaders
		{
			Accept = new[] { "json", "msgpack" }
		};
		Assert.Equal("json,msgpack", headers.Accept);
	}

	[Fact]
	public void SetExists_ShouldBeUpdated()
	{
		var headers = new FluentHttpHeaders
		{
			Accept = new[] { "json", "msgpack" }
		};
		headers.Accept = "json";
		Assert.Equal("json", headers.Accept);
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
		Assert.Equal("the-xx", headers.Authorization);
		Assert.Equal("json", headers.ContentType);
		Assert.Equal(headersMap.Count, headers.Count);
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
		Assert.Equal("the-xx", headers.Authorization);
		Assert.Equal("json", headers.ContentType);
		Assert.Equal(headersMap.Count, headers.Count);
	}

	[Fact]
	public void ShouldInitializeFromDictionaryOfEnumerableString()
	{
		var headersMap = new Dictionary<string, IEnumerable<string>>
		{
			{HeaderTypes.Accept, new[] {"json", "msgpack"} },
			{HeaderTypes.XForwardedFor, new[] {"192.168.1.1", "127.0.0.1"} },
		};

		var headers = new FluentHttpHeaders(headersMap);
		Assert.Equal("json,msgpack", headers.Accept);
		Assert.Equal("192.168.1.1,127.0.0.1", headers.XForwardedFor);
		Assert.Equal(headersMap.Count, headers.Count);
	}

	[Fact]
	public void ShouldInitializeFromHttpHeaders()
	{
		var httpHeaders = new HttpRequestMessage().Headers;
		httpHeaders.Add(HeaderTypes.Authorization, "the-xx");
		httpHeaders.Add(HeaderTypes.XForwardedFor, new[] { "192.168.1.1", "127.0.0.1" });

		var headers = new FluentHttpHeaders(httpHeaders);
		Assert.Equal("the-xx", headers.Authorization);
		Assert.Equal("192.168.1.1,127.0.0.1", headers.XForwardedFor);
		Assert.Equal(2, headers.Count);
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

		Assert.Equal("Authorization=the-xx&Content-Type=json", hash);
	}

	[Fact]
	public void ShouldHashWithEnumerable()
	{
		var headers = new FluentHttpHeaders
		{
			{HeaderTypes.Authorization, "the-xx"},
			{HeaderTypes.Accept, new[] {"json", "msgpack"}}
		};
		var hash = headers.ToHashString();

		Assert.Equal("Authorization=the-xx&Accept=json,msgpack", hash);
	}

	[Fact]
	public void ShouldFilterWithHashingFilter()
	{
		var headers = new FluentHttpHeaders
			{
				{HeaderTypes.Authorization, "the-xx"},
				{HeaderTypes.Accept, new[] {"json", "msgpack"}},
				{HeaderTypes.XForwardedHost, "sketch7.com"},
			}
			.WithOptions(opts => opts.WithHashingExclude(pair => pair.Key == HeaderTypes.Authorization));
		var hash = headers.ToHashString();

		Assert.Equal("Accept=json,msgpack&X-Forwarded-Host=sketch7.com", hash);
	}
}