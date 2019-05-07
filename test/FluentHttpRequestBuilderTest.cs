using FluentlyHttpClient;
using FluentlyHttpClient.Test;
using Microsoft.Extensions.Primitives;
using RichardSzalay.MockHttp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using static FluentlyHttpClient.Test.ServiceTestUtil;

// ReSharper disable InconsistentNaming
namespace Test
{
	public class RequestBuilder_Build
	{
		[Fact]
		public async Task WithoutUrl_ShouldUseBaseUrl()
		{
			var response = await GetNewClientFactory().CreateBuilder("abc")
				.WithBaseUrl("https://sketch7.com/api/heroes")
				.WithMessageHandler(new MockHttpMessageHandler())
				.Build()
				.CreateRequest()
				.ReturnAsResponse();

			Assert.Equal("https://sketch7.com/api/heroes/", response.Message.RequestMessage.RequestUri.ToString());
		}

		[Fact]
		public async Task SubClientWithoutUrl_ShouldUseBaseUrl()
		{
			var response = await GetNewClientFactory().CreateBuilder("abc")
				.WithBaseUrl("https://sketch7.com/api/heroes")
				.WithMessageHandler(new MockHttpMessageHandler())
				.Build()
				.CreateClient("sub")
				.WithBaseUrl("v1", replace: false)
				.Build()
				.CreateRequest()
				.ReturnAsResponse();

			Assert.Equal("https://sketch7.com/api/heroes/v1/", response.Message.RequestMessage.RequestUri.ToString());
		}

		[Fact]
		public async Task WithoutUrlAndWithQueryString_ShouldInterpolateQueryString()
		{
			var response = await GetNewClientFactory().CreateBuilder("abc")
				.WithBaseUrl("https://sketch7.com/api/heroes/")
				.WithMessageHandler(new MockHttpMessageHandler())
				.Build()
				.CreateRequest()
				.WithQueryParams(new { Language = "en" })
				.ReturnAsResponse();

			Assert.Equal("https://sketch7.com/api/heroes/?language=en", response.Message.RequestMessage.RequestUri.ToString());
		}
	}

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
				}, opts => opts.CollectionMode = QueryStringCollectionMode.CommaSeparated
				).Build();

			Assert.Equal("/org/sketch7/heroes?roles=warrior,assassin", request.Uri.ToString());
		}

		[Fact]
		public void InheritOptions_AsDefaults()
		{
			var builder = GetNewRequestBuilder();
			var request = builder.WithUri("/org/sketch7/heroes")
				.WithQueryParamsOptions(opts => opts.CollectionMode = QueryStringCollectionMode.CommaSeparated)
				.WithQueryParams(new
				{
					Roles = new List<string> { "warrior", "assassin" },
				}, opts => opts.KeyFormatter = s => s.ToUpper()
				).Build();

			Assert.Equal("/org/sketch7/heroes?ROLES=warrior,assassin", request.Uri.ToString());
		}

		[Fact]
		public void WithQueryParamsOptions_MultipleCallsShouldKeepConfiguring()
		{
			var builder = GetNewRequestBuilder();
			var request = builder.WithUri("/org/sketch7/heroes")
				.WithQueryParamsOptions(opts => opts.CollectionMode = QueryStringCollectionMode.CommaSeparated)
				.WithQueryParamsOptions(opts => opts.KeyFormatter = s => s.ToUpper())
				.WithQueryParams(new
				{
					Roles = new List<string> { "warrior", "assassin" },
				}
				).Build();

			Assert.Equal("/org/sketch7/heroes?ROLES=warrior,assassin", request.Uri.ToString());
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
		public void AddHeaderStringValues()
		{
			var builder = GetNewRequestBuilder()
					.WithUri("/org/sketch7")
					.WithHeader("locale", new StringValues(new[] { "mt", "en" }))
				;
			var request = builder.Build();

			var headers = request.Headers.GetValues("locale");
			Assert.NotNull(headers);
			Assert.Collection(
				headers,
				x => Assert.Equal("mt", x),
				x => Assert.Equal("en", x)
			);
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
		public void AddHeadersStringValues()
		{
			var builder = GetNewRequestBuilder()
					.WithUri("/org/sketch7")
					.WithHeaders(new Dictionary<string, StringValues>
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

	public class FluentRequest_GetHash
	{
		private static IFluentHttpClient GetClient()
		{
			var clientBuilder = GetNewClientFactory().CreateBuilder("sketch7")
					.WithBaseUrl("http://local.sketch7.io:5000")
					.WithHeader("locale", "en-GB")
					.WithHeader("X-SSV-VERSION", "2019.02-2")
				;
			return clientBuilder.Build();
		}

		[Fact]
		public void ShouldBeEqual()
		{
			var httpClient = GetClient();

			var request1 = httpClient.CreateRequest("/api/heroes/azmodan");
			var request2 = httpClient.CreateRequest("/api/heroes/azmodan");

			var request1Hash = request1.Build().GetHash();
			var request2Hash = request2.Build().GetHash();

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

			var requestHashWithHeaders = requestWithHeaders.Build().GetHash();
			var noTokenRequestHash = requestNoHeaders.Build().GetHash();

			Assert.NotEqual(requestHashWithHeaders, noTokenRequestHash);

			const string requestHashWithHeadersAssert =
				"method=GET;url=http://local.sketch7.io:5000/api/heroes/azmodan;headers=Accept=application/json,text/json,application/xml,text/xml,application/x-www-form-urlencoded&User-Agent=fluently&locale=en-GB&X-SSV-VERSION=2019.02-2&Authorization=Bearer XXX&local=en-GB;content=";
			const string noHeadersRequestHashAssert =
				"method=GET;url=http://local.sketch7.io:5000/api/heroes/azmodan;headers=Accept=application/json,text/json,application/xml,text/xml,application/x-www-form-urlencoded&User-Agent=fluently&locale=en-GB&X-SSV-VERSION=2019.02-2;content=";
			Assert.Equal(requestHashWithHeadersAssert, requestHashWithHeaders);
			Assert.Equal(noHeadersRequestHashAssert, noTokenRequestHash);
		}

		public class WithHashingOptions
		{
			[Fact]
			public void WithHeadersExclude_ShouldExclude()
			{
				var requestBuilder = GetNewRequestBuilder()
						.WithRequestHashOptions(opts =>
							opts.WithHeadersExclude(pair => pair.Key == HeaderTypes.Authorization))
						.WithUri("/api/heroes/azmodan")
						.WithBearerAuthentication("XXX")
						.WithHeader("local", "en-GB")
					;

				var hash = requestBuilder.Build().GetHash();

				const string assertHash = "method=GET;url=https://sketch7.com/api/heroes/azmodan;headers=Accept=application/json,text/json,application/xml,text/xml,application/x-www-form-urlencoded&User-Agent=fluently&local=en-GB;content=";
				Assert.Equal(assertHash, hash);
			}

			[Fact]
			public void WithHeadersExcludeByKey_ShouldExclude()
			{
				var requestBuilder = GetNewRequestBuilder()
						.WithRequestHashOptions(opts => opts.WithHeadersExcludeByKey(HeaderTypes.Accept))
						.WithUri("/api/heroes/azmodan")
						.WithHeader("local", "en-GB")
					;

				var hash = requestBuilder.Build().GetHash();

				const string assertHash =
					"method=GET;url=https://sketch7.com/api/heroes/azmodan;headers=User-Agent=fluently&local=en-GB;content=";
				Assert.Equal(assertHash, hash);
			}

			[Fact]
			public void WithHeadersExcludeByKeys_ShouldExclude()
			{
				var requestBuilder = GetNewRequestBuilder()
						.WithRequestHashOptions(opts =>
							opts.WithHeadersExcludeByKeys(new[] { HeaderTypes.Accept, HeaderTypes.UserAgent }))
						.WithUri("/api/heroes/azmodan")
						.WithHeader("local", "en-GB")
					;

				var hash = requestBuilder.Build().GetHash();

				const string assertHash = "method=GET;url=https://sketch7.com/api/heroes/azmodan;headers=local=en-GB;content=";
				Assert.Equal(assertHash, hash);
			}

			[Fact]
			public void WithHeadersExclude_ShouldCombinedExclusions()
			{
				var requestBuilder = GetNewRequestBuilder(configureClient: clientBuilder =>
				{
					clientBuilder.WithRequestBuilderDefaults(rb =>
					{
						rb.WithRequestHashOptions(opts =>
						{
							opts.WithHeadersExclude(pair => pair.Key == HeaderTypes.Authorization)
								.WithHeadersExclude(pair => pair.Key == HeaderTypes.Accept);
						});
					});
				}).WithRequestHashOptions(opts =>
						opts.WithHeadersExclude(pair => pair.Key == HeaderTypes.UserAgent))
						.WithUri("/api/heroes/azmodan")
						.WithBearerAuthentication("XXX")
						.WithHeader("local", "en-GB")
					;

				var hash = requestBuilder.Build().GetHash();

				Assert.Equal("method=GET;url=https://sketch7.com/api/heroes/azmodan;headers=local=en-GB;content=", hash);
			}

			[Fact]
			public void WithUri_ShouldRemoveQuerystringParam()
			{
				var requestBuilder = GetNewRequestBuilder()
						.WithRequestHashOptions(opts => opts.WithUri(uri =>
						{
							var ub = new UriBuilder(uri)
								.ManipulateQueryString(c => c.Remove("token"));
							return ub.Uri.ToString();
						}))
						.WithUri("/api/heroes/azmodan")
						.WithQueryParams(new { token = "XXX" })
					;

				var hash = requestBuilder.Build().GetHash();

				const string assertHash = "method=GET;url=https://sketch7.com/api/heroes/azmodan;headers=Accept=application/json,text/json,application/xml,text/xml,application/x-www-form-urlencoded&User-Agent=fluently;content=";
				Assert.Equal(assertHash, hash);
			}

			[Fact]
			public void WithUriQueryString_ShouldRemoveQuerystringParam()
			{
				var requestBuilder = GetNewRequestBuilder()
						.WithRequestHashOptions(opts => opts.WithUriQueryString(qs => qs.Remove("token")))
						.WithUri("/api/heroes/azmodan")
						.WithQueryParams(new { token = "XXX" })
					;

				var hash = requestBuilder.Build().GetHash();

				const string assertHash = "method=GET;url=https://sketch7.com/api/heroes/azmodan;headers=Accept=application/json,text/json,application/xml,text/xml,application/x-www-form-urlencoded&User-Agent=fluently;content=";
				Assert.Equal(assertHash, hash);
			}

			[Fact]
			public void Body_ShouldBeIncluded()
			{
				var requestBuilder = GetNewRequestBuilder()
						.WithUri("/api/heroes/azmodan")
						.AsPost()
						.WithBody(new
						{
							email = "chiko@sketch7.com"
						})
					;

				var requestHash = requestBuilder.Build().GetHash();

				const string assertHash = "method=POST;url=https://sketch7.com/api/heroes/azmodan;headers=Accept=application/json,text/json,application/xml,text/xml,application/x-www-form-urlencoded&User-Agent=fluently;content={\"email\":\"chiko@sketch7.com\"}";
				Assert.Equal(assertHash, requestHash);
			}

			[Fact]
			public void WithBodyInvariant_BodyShouldBeExcluded()
			{
				var requestBuilder = GetNewRequestBuilder()
						.WithRequestHashOptions(opts => opts.WithBodyInvariant())
						.WithUri("/api/heroes/azmodan")
						.AsPost()
						.WithBody(new
						{
							email = "chiko@sketch7.com"
						})
					;

				var hash = requestBuilder.Build().GetHash();

				const string assertHash = "method=POST;url=https://sketch7.com/api/heroes/azmodan;headers=Accept=application/json,text/json,application/xml,text/xml,application/x-www-form-urlencoded&User-Agent=fluently;content=";
				Assert.Equal(assertHash, hash);
			}
		}
	}
}