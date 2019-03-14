﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using FluentlyHttpClient;
using FluentlyHttpClient.Test;
using RichardSzalay.MockHttp;
using Xunit;
using static FluentlyHttpClient.Test.ServiceTestUtil;

// ReSharper disable InconsistentNaming
namespace Test
{
	public class RequestBuilder_WithUri
	{
		[Fact]
		public void ShouldInterpolate()
		{
			var request = GetNewRequestBuilder()
				.WithUri("{Language}/heroes/{Hero}", new
				{
					Language = "en",
					Hero = "azmodan"
				}).Build();

			Assert.Equal("en/heroes/azmodan", request.Uri.ToString());
		}

		[Fact]
		public void NullValue_ShouldThrow()
		{
			var requestBuilder = GetNewRequestBuilder();
			Assert.Throws<ArgumentNullException>("args", () => requestBuilder.WithUri("{Language}/heroes", new
			{
				Language = (string)null
			}));
		}
	}

	public class RequestBuilder_WithQueryParams
	{
		[Fact]
		public void AddQuery()
		{
			var builder = GetNewRequestBuilder();
			var request = builder.WithUri("/org/sketch7")
				.WithQueryParams(new
				{
					Page = 1,
					Filter = "all"
				}).Build();

			Assert.Equal("/org/sketch7?page=1&filter=all", request.Uri.ToString());
		}

		[Fact]
		public void AddWithoutAsModel()
		{
			var builder = GetNewRequestBuilder();
			var request = builder.WithUri("/org/sketch7")
				.WithQueryParams(new
				{
					Page = 1,
					Filter = "all"
				}, c => c.KeyFormatter = null).Build();

			Assert.Equal("/org/sketch7?Page=1&Filter=all", request.Uri.ToString());
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		public void NullOrEmptyValue_RemainAsIs(string data)
		{
			string filter = data;
			var builder = GetNewRequestBuilder();
			var request = builder.WithUri("/org/sketch7")
				.WithQueryParams(new
				{
					filter,
				}).Build();

			Assert.Equal("/org/sketch7", request.Uri.ToString());
		}

		[Fact]
		public void NullValue_Omitted()
		{
			string filter = null;
			var builder = GetNewRequestBuilder();
			var request = builder.WithUri("/org/sketch7")
				.WithQueryParams(new
				{
					filter,
					Page = 1
				}).Build();

			Assert.Equal("/org/sketch7?page=1", request.Uri.ToString());
		}

		[Fact]
		public void AppendQuery()
		{
			var builder = GetNewRequestBuilder();
			var request = builder.WithUri("/org/sketch7?hero=rex")
				.WithQueryParams(new
				{
					Page = 1,
					Filter = "all"
				}).Build();

			Assert.Equal("/org/sketch7?hero=rex&page=1&filter=all", request.Uri.ToString());
		}

		[Fact]
		public void EmptyObject_RemainAsIs()
		{
			var builder = GetNewRequestBuilder();
			var request = builder.WithUri("/org/sketch7")
				.WithQueryParams(new { })
				.Build();

			Assert.Equal("/org/sketch7", request.Uri.ToString());
		}

		[Fact]
		public void CollectionQueryString()
		{
			var builder = GetNewRequestBuilder();
			var request = builder.WithUri("/org/sketch7/heroes")
				.WithQueryParams(new
				{
					Roles = new List<string> { "warrior", "assassin" },
					Powers = new List<int> { 1337, 2337 }
				}).Build();

			Assert.Equal("/org/sketch7/heroes?roles=warrior&roles=assassin&powers=1337&powers=2337", request.Uri.ToString());
		}

		[Fact]
		public void CollectionQueryString_CommaSeparated()
		{
			var builder = GetNewRequestBuilder();
			var request = builder.WithUri("/org/sketch7/heroes")
				.WithQueryParams(new
				{
					Roles = new List<string> { "warrior", "assassin" },
				},
				 opts => opts.CollectionMode = QueryStringCollectionMode.CommaSeparated
				).Build();

			Assert.Equal("/org/sketch7/heroes?roles=warrior,assassin", request.Uri.ToString());
		}
	}

	public class RequestBuilder_BuildValidation
	{
		[Fact]
		public void ThrowsErrorWhenMethodNotSpecified()
		{
			var builder = GetNewRequestBuilder();
			Assert.Throws<RequestValidationException>(() => builder.WithMethod(null).WithUri("/org").Build());
		}

		[Fact]
		public void ThrowsErrorWhenUriNotSpecified()
		{
			var builder = GetNewRequestBuilder(uri: null);
			Assert.Throws<RequestValidationException>(() => builder.Build());
		}

		[Fact]
		public void ThrowsErrorWhenGetAndHasBodySpecified()
		{
			var requestBuilder = GetNewRequestBuilder()
				.AsGet()
				.WithBody(new { Name = "Kaboom!" });
			Assert.Throws<RequestValidationException>(() => requestBuilder.Build());
		}
	}

	public class RequestBuilder_WithHeaders
	{
		[Fact]
		public void AddHeader()
		{
			var builder = GetNewRequestBuilder()
				.WithUri("/org/sketch7")
				.WithHeader("chiko", "hex")
				;
			var request = builder.Build();

			var header = request.Headers.GetValues("chiko").FirstOrDefault();
			Assert.NotNull(header);
			Assert.Equal("hex", header);
		}

		[Fact]
		public void AddAlreadyExistsHeader_ShouldReplace()
		{
			var builder = GetNewRequestBuilder()
					.WithUri("/org/sketch7")
					.WithHeader("chiko", "hex")
					.WithHeader("chiko", "hexII")
				;
			var request = builder.Build();

			var header = request.Headers.GetValues("chiko").FirstOrDefault();
			Assert.NotNull(header);
			Assert.Equal("hexII", header);
		}

		[Fact]
		public void AddHeaders()
		{
			var builder = GetNewRequestBuilder()
					.WithUri("/org/sketch7")
					.WithHeader("chiko", "hex")
					.WithHeaders(new Dictionary<string, string>
					{
						["chiko"] = "hexII",
						["locale"] = "mt-MT"
					})
				;
			var request = builder.Build();

			var chikoHeader = request.Headers.GetValues("chiko").FirstOrDefault();
			var localeHeader = request.Headers.GetValues("locale").FirstOrDefault();
			Assert.NotNull(chikoHeader);
			Assert.Equal("hexII", chikoHeader);
			Assert.NotNull(localeHeader);
			Assert.Equal("mt-MT", localeHeader);
		}

		[Fact]
		public void WithUserAgent()
		{
			var builder = GetNewRequestBuilder()
					.WithUri("/org/sketch7")
					.WithUserAgent("fluently")
				;
			var request = builder.Build();

			var userAgentHeader = request.Headers.GetValues(HeaderTypes.UserAgent).FirstOrDefault();
			Assert.NotNull(userAgentHeader);
			Assert.Equal("fluently", userAgentHeader);
		}

		[Fact]
		public void WithUserAgent_WeirdButValid()
		{
			const string userAgent = "Mozilla/5.0 (Linux; Android 6.0; vivo 1601 Build/MRA58K; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/63.0.3239.111 Mobile Safari/537.36 [FB_IAB/FB4A;FBAV/153.0.0.53.88;]";
			var builder = GetNewRequestBuilder()
					.WithUri("/org/sketch7")
					.WithUserAgent(userAgent)
				;
			var request = builder.Build();

			var userAgentHeader = request.Headers.GetValues(HeaderTypes.UserAgent).FirstOrDefault();
			Assert.NotNull(userAgentHeader);
			Assert.Equal(userAgent, userAgentHeader);
		}
	}

	public class RequestBuilder_Return
	{
		[Fact]
		public async void ReturnAsString()
		{
			var mockResponse = "{ 'name': 'Azmodan' }";
			var mockHttp = new MockHttpMessageHandler();
			mockHttp.When("https://sketch7.com/api/heroes/azmodan")
				.Respond("application/json", mockResponse);

			var httpClient = GetNewClientFactory()
				.CreateBuilder("sketch7")
				.WithBaseUrl("https://sketch7.com")
				.WithMessageHandler(mockHttp)
				.Build();

			var response = await httpClient.CreateRequest("/api/heroes/azmodan")
				.ReturnAsString();

			Assert.NotNull(response);
			Assert.Equal(mockResponse, response);
		}
	}

	public class RequestBuilder_PostWithBody
	{
		[Fact]
		public async void ReturnAsTypedObject()
		{
			var mockHttp = new MockHttpMessageHandler();
			mockHttp.When(HttpMethod.Post, "https://sketch7.com/api/heroes/azmodan")
				.WithContent("{\"title\":\"Lord of Sin\"}")
				.Respond("application/json", "{ 'name': 'Azmodan', 'title': 'Lord of Sin' }");

			var httpClient = GetNewClientFactory()
				.CreateBuilder("sketch7")
				.WithBaseUrl("https://sketch7.com")
				.WithMessageHandler(mockHttp)
				.Build();

			var response = await httpClient.CreateRequest("/api/heroes/azmodan")
				.AsPost()
				.WithBody(new
				{
					Title = "Lord of Sin"
				})
				.ReturnAsResponse<Hero>();

			response.EnsureSuccessStatusCode();
			Assert.NotNull(response);
			Assert.Equal("Lord of Sin", response.Data.Title);
		}
	}

	public class RequestBuilder_ReturnMulti
	{
		[Fact]
		public async void ShouldSendTwoRequests()
		{
			var mockHttp = new MockHttpMessageHandler();
			mockHttp.When(HttpMethod.Post, "https://sketch7.com/api/heroes/azmodan")
				.Respond("application/json", "{ 'name': 'Azmodan', 'title': 'Lord of Sin' }");

			var fluentHttpClientFactory = GetNewClientFactory();
			fluentHttpClientFactory.CreateBuilder("sketch7")
				.WithBaseUrl("https://sketch7.com")
				.WithMessageHandler(mockHttp)
				.UseLogging()
				.UseTimer()
				.Register();

			var httpClient = fluentHttpClientFactory.Get("sketch7");
			var requestBuilder = httpClient.CreateRequest("/api/heroes/azmodan")
				.AsPost()
				.WithBody(new
				{
					Title = "Lord of Sin"
				});
			var response1 = await requestBuilder.ReturnAsResponse<Hero>();
			var response2 = await requestBuilder.ReturnAsResponse<Hero>();

			response1.EnsureSuccessStatusCode();
			Assert.NotNull(response1);
			Assert.Equal("Lord of Sin", response1.Data.Title);

			response2.EnsureSuccessStatusCode();
			Assert.NotNull(response2);
			Assert.Equal("Lord of Sin", response2.Data.Title);
		}
	}

	public class HttpMessage_GenerateHash
	{
		private static IFluentHttpClient GetClient()
		{
			var fluentHttpClientFactory = GetNewClientFactory();
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
			var httpClient = GetClient();

			var request1 = httpClient.CreateRequest("/api/heroes/azmodan");
			var request2 = httpClient.CreateRequest("/api/heroes/azmodan");

			var request1Hash = request1.Build().GenerateHash();
			var request2Hash = request2.Build().GenerateHash();

			Assert.Equal(request1Hash, request2Hash);
		}

		[Fact]
		public void ShouldHandleHeaders()
		{
			var httpClient = GetClient();

			var requestWithHeaders = httpClient.CreateRequest("/api/heroes/azmodan")
				.WithBearerAuthentication("XXX")
				.WithHeader("local", "en-GB")
				;
			var requestNoHeaders = httpClient.CreateRequest("/api/heroes/azmodan");

			var requestHashWithHeaders = requestWithHeaders.Build().GenerateHash();
			var noTokenRequestHash = requestNoHeaders.Build().GenerateHash();

			Assert.NotEqual(requestHashWithHeaders, noTokenRequestHash);

			const string requestHashWithHeadersAssert = "method=GET;url=http://local.sketch7.io:5000/api/heroes/azmodan;headers=Accept=application/json,text/json,application/xml,text/xml,application/x-www-form-urlencoded&User-Agent=fluently&locale=en-GB&X-SSV-VERSION=2019.02-2&Authorization=Bearer XXX&local=en-GB";
			const string noHeadersRequestHashAssert = "method=GET;url=http://local.sketch7.io:5000/api/heroes/azmodan;headers=Accept=application/json,text/json,application/xml,text/xml,application/x-www-form-urlencoded&User-Agent=fluently&locale=en-GB&X-SSV-VERSION=2019.02-2";
			Assert.Equal(requestHashWithHeadersAssert, requestHashWithHeaders);
			Assert.Equal(noHeadersRequestHashAssert, noTokenRequestHash);
		}

		[Fact]
		public void WithHashingOptions_ShouldExcludeHeaders()
		{
			var requestWithHeaders = GetNewRequestBuilder()
				.WithRequestHashOptions(opts => opts.WithHeadersExclude(pair => pair.Key == HeaderTypes.Authorization))
				.WithUri("/api/heroes/azmodan")
				.WithBearerAuthentication("XXX")
				.WithHeader("local", "en-GB")
				;

			var requestHashWithHeaders = requestWithHeaders.Build().GenerateHash();

			const string requestHashWithHeadersAssert = "method=GET;url=https://sketch7.com/api/heroes/azmodan;headers=Accept=application/json,text/json,application/xml,text/xml,application/x-www-form-urlencoded&User-Agent=fluently&local=en-GB";
			Assert.Equal(requestHashWithHeadersAssert, requestHashWithHeaders);
		}

		[Fact]
		public void WithHashingOptions_ShouldExcludeHeadersCombined()
		{
			var requestWithHeaders = GetNewRequestBuilder(configureClient: clientBuilder =>
						{
							clientBuilder.WithRequestBuilderDefaults(requestBuilder =>
								{
									requestBuilder.WithRequestHashOptions(opts =>
									{
										//opts.UriManipulation = uri => "";
										opts.WithHeadersExclude(pair => pair.Key == HeaderTypes.Authorization)
											.WithHeadersExclude(pair => pair.Key == HeaderTypes.Accept);
									});
								});
						})
				.WithRequestHashOptions(opts => opts.WithHeadersExclude(pair => pair.Key == HeaderTypes.UserAgent))
				.WithUri("/api/heroes/azmodan")
				.WithBearerAuthentication("XXX")
				.WithHeader("local", "en-GB")
				;

			var hash = requestWithHeaders.Build().GenerateHash();

			Assert.Equal("method=GET;url=https://sketch7.com/api/heroes/azmodan;headers=local=en-GB", hash);
		}
	}
}