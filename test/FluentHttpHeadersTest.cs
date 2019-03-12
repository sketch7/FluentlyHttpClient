using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace FluentlyHttpClient.Test
{
	public class FluentHttpHeaders_Add
	{
		[Fact]
		public void ShouldAddSingle()
		{
			var headers = new FluentHttpHeaders();
			headers.Add(HeaderTypes.Authorization, "the-xx");
			Assert.Equal("the-xx", headers.Authorization);
		}

		[Fact]
		public void ShouldAddEnumerable()
		{
			var headers = new FluentHttpHeaders();
			headers.Add(HeaderTypes.Accept, new[] { "json", "msgpack" });
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
			var headers = new FluentHttpHeaders
			{
				{HeaderTypes.Accept, new[] {"json", "msgpack"}}
			};
			Assert.Equal("json,msgpack", headers.Accept);
		}

		[Fact]
		public void SetNotExists_ShouldBeAdded()
		{
			var headers = new FluentHttpHeaders();
			headers.Accept = new[] { "json", "msgpack" };
			Assert.Equal("json,msgpack", headers.Accept);
		}
		[Fact]
		public void SetExists_ShouldBeUpdated()
		{
			var headers = new FluentHttpHeaders();
			headers.Accept = new[] { "json", "msgpack" };
			headers.Accept = "json";
			Assert.Equal("json", headers.Accept);
		}
	}

	public class FluentHttpHeaders_Initialize
	{
		[Fact]
		public void ShouldInitializeFromDictionary()
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
		public void ShouldInitializeFromDictionaryEnumerableValue()
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
				{HeaderTypes.ContentType, "json" }
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
	}
}
