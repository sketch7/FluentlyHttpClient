# Fluently Http Changelog

[_vNext_](https://github.com/sketch7/FluentlyHttpClient/compare/3.6.0...3.7.0) (2019-X-X)

## [3.7.0](https://github.com/sketch7/FluentlyHttpClient/compare/3.6.0...3.7.0) (2019-09-09)

### Features

- **logging:** add headers to log message in `LoggerHttpMiddleware`

## [3.6.0](https://github.com/sketch7/FluentlyHttpClient/compare/3.5.0...3.6.0) (2019-09-04)

### Features

- **headers:** add several utility methods 
  - `ctor IDictionary<string, StringValues>`
  - `Add(string, StringValues)`
  - `Remove(string)`
  - `Contains(string)`
- **util:** querystring options `WithValueFormatter` now sets also `WithCollectionItemFormatter`
- **util:** querystrings now ignore `private`, `protected` and properties marked with `IgnoreDataMemberAttribute` when using `.WithQueryParams` or so

### Deprecated code

- **util:** `WithCollectionItemFormatter` deprecated in favor of `WithValueFormatter` as it will work for all query string values - collection items and props

## [3.5.0](https://github.com/sketch7/FluentlyHttpClient/compare/3.4.0...3.5.0) (2019-09-03)

### Features

- **util:** add querystring option `WithValueFormatter` similar to `WithCollectionItemFormatter` to format each value e.g. enum formatter

### Deprecated code

- **util:** query options deprecate `CollectionItemFormatter` and `KeyFormatter` with `WithCollectionItemFormatter`, `WithKeyFormatter` instead of exposing func directly

## [3.4.0](https://github.com/sketch7/FluentlyHttpClient/compare/3.3.2...3.4.0) (2019-08-26)

### Features

- **http client builder:** add `WithBaseUrlTrailingSlash` to control whether to include trailing slashes or not in base url

## [3.3.2](https://github.com/sketch7/FluentlyHttpClient/compare/3.3.1...3.3.2) (2019-08-20)

### Bug Fixes

- **http request:** `HttpRequestMessage.Properties` were not being flown to `FluentHttpRequest` when using raw client
- **http client:** fix issue when using `Send(HttpRequestMessage request)` (raw send) was creating `FluentHttpResponse` twice and the second which is returned was not including the `.Items`

## [3.3.1](https://github.com/sketch7/FluentlyHttpClient/compare/3.3.0...3.3.1) (2019-06-12)

### Bug Fixes

- **http request:** add `Items` from `FluentHttpRequestBuilder` which where missing when executing requests using `FluentHttpClient.RawHttpClient`

## [3.3.0](https://github.com/sketch7/FluentlyHttpClient/compare/3.2.1...3.3.0) (2019-05-29)

### Features

- **http client:** implicit conversion between `FluentHttpClient` and `HttpClient`
- **http client:** retain middleware logic when using `FluentHttpClient.RawHttpClient`
- **http client:** add overload `Send(HttpRequestMessage request)` for better interoperability with the native when already having `HttpRequestMessage`
- **headers:** `FluentHttpHeaders` now implements `IFluentHttpHeaderBuilder`, any extension methods added to `IFluentHttpHeaderBuilder` will be available for `FluentHttpHeaders` also, along with `FluentHttpClientBuilder` and `FluentHttpRequestBuilder`
- **middleware context:** `FluentHttpMiddlewareClientContext` now includes formatters

### Deprecated code

- **request:** `FluentHttpRequest.Formatters` is now deprecated, this was added to be passed down to middleware. Now instead use the `FluentHttpMiddlewareClientContext.Formatters`

## [3.2.1](https://github.com/sketch7/FluentlyHttpClient/compare/3.2.0...3.2.1) (2019-05-21)

### Bug Fixes

- **http client builder:** fix formatters' merging when creating sub clients

## [3.2.0](https://github.com/sketch7/FluentlyHttpClient/compare/3.1.1...3.2.0) (2019-05-07)

### Features

- **request builder:** when using `WithQueryParams` and configure options or configure options with `WithQueryParamsOptions` will inherit from the previous configured options instead of the defaults

## [3.1.1](https://github.com/sketch7/FluentlyHttpClient/compare/3.1.0...3.1.1) (2019-05-02)

### Features

- **http client:** client identifier inherits parent's e.g. `{parentId}.{id}` in the future this will be configurable

### Bug Fixes

- **request builder:** fix request without path to build full uri with base correctly

## [3.1.0](https://github.com/sketch7/FluentlyHttpClient/compare/3.1.0...3.1.0) (2019-04-25)

### Features

- **request builder:** now supports request without `Uri` to use only `BaseUri`

## [3.0.1](https://github.com/sketch7/FluentlyHttpClient/compare/3.0.0...3.0.1) (2019-04-12)

### Bug Fixes

- **headers:** `_data` was `public` now is set to `private` as intended

## [3.0.0](https://github.com/sketch7/FluentlyHttpClient/compare/2.2.0...3.0.0) (2019-04-12)

Middleware has been reworked, its now much more efficient as it creates instances per middleware per client, instead of every request.

### Features

- **middleware:** middleware has been reworked, and now are much more efficient
- **middleware:** middleware now logs the identifier in source e.g. `FluentlyHttpClient.Middleware.sketch7.Timer`, given HttpClient identifier was `sketch7`,
  so logs can be more fine controlled
- **headers:** add `FluentHttpHeaders` since `HttpHeaders` (and all implementations) cannot create instances of, and changed all implementations to use it instead of `Dictionary<string, string>`
- **util:** add several extensions when working with `HttpHeaders`
- **http request:** now exposes the request builder
- **http request:** add request hash `FluentHttpRequest.GetHash`, `FluentHttpRequestBuilder.WithRequestHashOptions` which can be used to build an id hash for the request
- **request builder:** add `IFluentHttpMessageItems` which `FluentHttpRequestBuilder`, `FluentHttpRequest` and `FluentHttpResponse` implements, so items extension methods can target all
- **http client builder:** `WithBaseUrl` now accepts optional bool `replace` which will append to the existing. Useful when creating sub client to inherit and continue adding to it
- **response caching middleware:** implemented response caching middleware `UseResponseCaching` which the intention is not for performance improvements but more to proxy/mocking, as it copies responses and serve them if they were already requested.
  We also implemented remote caching which stores in database using EntityFramework see [FluentlyHttpClient.Entity](./src/FluentlyHttpClient.Entity/README.md). It can also be extended to implement custom stores e.g. we have another which is for Microsoft Orleans (not published - yet)

### Bug Fixes

- **http client:** fix `identifier` for sub client when using `CreateClient` was being replaced by the parent's instead of the one specified
- **http client builder:** fix an issue when base url doesn't contain a trailing `/` in certain cases it will trim last value e.g. http://myapi.com/v1 would result in http://myapi.com

### BREAKING CHANGES

Even though breaking changes are quite a lot, most of them are more internal

- **middleware:** `FluentHttpRequestDelegate` has been removed in favor of `FluentHttpMiddlewareDelegate`
- **middleware:** `IFluentHttpMiddlewareRunner` and `FluentHttpMiddlewareRunner` has been reworked
- **middleware:** `IFluentHttpMiddleware` changed from `Invoke(FluentHttpRequest)` to `Invoke(FluentHttpMiddlewareContext)`
- **middleware:** renamed `MiddlewareConfig` to `FluentHttpMiddlewareConfig` for consistency
- **http client builder:** `FluentHttpClientOptions` changed `List<MiddlewareConfig> Middleware` to `FluentHttpMiddlewareBuilder MiddlewareBuilder`
- **http client factory:** removed `Add(FluentHttpClientOptions)` from `IFluentHttpClientFactory` and moved it as an extension method
- **exceptions:** exceptions has been moved correctly to `FluentlyHttpClient` namespace
- **util:** querystrings now defaults to camel casing instead of lower casing
- **request builder:** `FluentHttpRequestBuilder.WithQueryParams(object, bool)` has been removed
- **request builder:** change headers from `Dictionary<string, string>` to `FluentHttpHeaders`
- **http client builder:** change options headers from `Dictionary<string, string>` to `FluentHttpHeaders`

See [these changes](https://github.com/sketch7/FluentlyHttpClient/pull/25/files#diff-efc205ab9587ec42db3de44d14c0ce86)
in order to help you update an existing middleware to the new version (fear not, changes are minimal :)).

## [2.2.0](https://github.com/sketch7/FluentlyHttpClient/compare/2.1.2...2.2.0) (2019-03-03)

- **http client:** now able to create a new http client from an existing one and inheriting options with `CreateClient`
- **http client builder:** add `FromOptions` which gets configured via `FluentHttpClientOptions`
- **http client builder:** rename `Build` to `BuildOptions` and `Build` now returns the http client
- **http client builder:** `WithRequestBuilderDefaults` now will combine previous defaults instead of replacing them.
  If you want to replace the previous defaults (as it was working), use `WithRequestBuilderDefaults(..., replace: true)`.
  This behavior is changed because its more expected that they are combined, especially when creating a sub-client and adding/changing request defaults would loose the previous defaults from the creator (parent).
- **http client factory:** add overload `Add(IFluentHttpClient)`

### BREAKING CHANGES

- **http client builder:** `Build` has been renamed to `BuildOptions` and add `Build` which now returns an Http Client.
  _Most probably it won't affect anyone since its more for internal use_

## [2.1.2](https://github.com/sketch7/FluentlyHttpClient/compare/2.1.1...2.1.2) (2018-10-24)

### Chore

- **build:** build correctly as `Release` mode

## [2.1.1](https://github.com/sketch7/FluentlyHttpClient/compare/2.1.0...2.1.1) (2018-10-13)

### Features

- **util:** expose FluentlyHttpClient version via `FluentlyHttpClientMeta.Version`

## [2.1.0](https://github.com/sketch7/FluentlyHttpClient/compare/2.0.1...2.1.0) (2018-09-14)

### Features

- **util:** implemented more powerful options to querystring

  - `CollectionMode` - this will allow to configure collection items in querystrings as below:
    - `KeyPerValue` e.g. `"filter=assassin&filter=fighter"`
    - `CommaSeparated` e.g. `"filter=assassin,fighter"`
  - `CollectionItemFormatter` - Allows you to format item value e.g. take enum description value. (see tests)
  - `KeyFormatter` - Allows you to format key with via a custom function e.g. lowercase, camelCase, PascalCase and you can utilize other libraries such as [humanizer](http://humanizr.net/).

- **request builder:** now using the new querystring options `WithQueryParams(object queryParams, QueryStringOptions options)` (and similar with `Action`)
- **request builder:** `WithQueryParamsOptions(QueryStringOptions options)` (and similar with `Action`) which can be used nicer in conjuction with `WithRequestBuilderDefaults` as below:

```cs
clientBuilder.WithRequestBuilderDefaults(builder =>
  builder.WithQueryParamsOptions(opts =>
  {
    opts.CollectionMode = QueryStringCollectionMode.CommaSeparated;
    opts.KeyFormatter = key => key.ToUpper();
  })
);
```

### Deprecated code

- **request builder:** `WithQueryParams(object queryParams, bool lowerCaseQueryKeys)` has been marked as obsolete, instead the newly `WithQueryParams(object queryParams, Action<QueryStringOptions> configure)`.
  e.g. `WithQueryParams(params, opts => opts.KeyFormatter = key => key.ToLower())`

## [2.0.1](https://github.com/sketch7/FluentlyHttpClient/compare/2.0.0...2.0.1) (2018-07-31)

### Bug Fixes

- **querystring builder:** fix issue with query string when have multiple values.

## [2.0.0](https://github.com/sketch7/FluentlyHttpClient/compare/1.4.5...2.0.0) (2018-06-24)

### Features

- **deps:** update to .net standard 2.0 + .net core 2.1.
- **deps:** remove `WinInsider.System.Net.Http.Formatting` and replaced with `Microsoft.AspNet.WebApi.Client`.
- **http client:** now using `IHttpClientFactory` inorder to create `HttpClient` (without message handler)
- **services:** `AddFluentlyHttpClient` now uses `TryAddSingleton` and registers `AddHttpClient`
- **request builder:** when building querystring now supports collections

  e.g. `Roles = new List<string> { "warrior", "assassin" }` => `roles=warrior&roles=assassin`

- **timer middleware:** add `UseTimer(this FluentHttpClientBuilder builder, Action<TimerHttpMiddlewareOptions> configure = null)` overload.
- **logger middleware:** add `WithLoggingOptions(this FluentHttpRequestBuilder builder, Action<LoggerHttpMiddlewareOptions> configure = null)` overload.
- **logger middleware:** add `UseLogging(this FluentHttpClientBuilder builder, Action<LoggerHttpMiddlewareOptions> configure = null)` overload.

### Changes

- **timer middleware:** increase warning threshold to `400`ms by default.

### BREAKING CHANGES

- Removed deprecated code
  - `IDictionary.Set`, `FluentHttpClientBuilder.Withdentifier` (typo) and `FluentHttpClientBuilder.WithFormatters`

## [1.4.4](https://github.com/sketch7/FluentlyHttpClient/compare/1.4.3...1.4.4) (2018-06-11)

### Bug Fixes

- **request builder:** fix issue with query string `.WithQueryParams` when value is empty string.

## [1.4.3](https://github.com/sketch7/FluentlyHttpClient/compare/1.4.2...1.4.3) (2018-04-07)

### Bug Fixes

- **request builder:** losen parsing for `userAgent` header, since errors are thrown when having user agent like the below which seems to be still valid.
  `Mozilla/5.0 (Linux; Android 6.0; vivo 1601 Build/MRA58K; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/63.0.3239.111 Mobile Safari/537.36 [FB_IAB/FB4A;FBAV/153.0.0.53.88;]`

## [1.4.2](https://github.com/sketch7/FluentlyHttpClient/compare/1.4.1...1.4.2) (2018-03-18)

### Features

- **timer middleware:** now logs even when an exception is thrown, which might be useful when a timeout has triggered.
- **timer middleware:** `SetTimeTaken` now returns `FluentHttpResponse` instead of `void`.

## [1.4.1](https://github.com/sketch7/FluentlyHttpClient/compare/1.4.0...1.4.1) (2018-03-04)

### Bug Fixes

- **request builder:** items within request/response e.g. time taken (from timer middleware) were updating the request builder items which
  was causing susequent requests built by the same request builder to fail.
- **http client builder:** `Withdentifier` has been renamed correctly to `WithIdentifier` previous one was marked as obsolete.

### Deprecated code

- **http client builder:** `Withdentifier` has been marked as obsolete, instead the newly `WithIdentifier`.

## [1.4.0](https://github.com/sketch7/FluentlyHttpClient/compare/1.3.0...1.4.0) (2018-03-03)

### Features

- **logger middleware:** add `LoggerHttpMiddlewareOptions` to configure `ShouldLogDetailedRequest` and `ShouldLogDetailedResponse`.
- **logger middleware:** add `WithLoggingOptions` on request builder, to specify options per request.
- **timer middleware:** add `WithTimerWarnThreshold` on request builder, to specify options per request.

## [1.3.0](https://github.com/sketch7/FluentlyHttpClient/compare/1.2.1...1.3.0) (2018-02-24)

### Features

- **http client builder:** add `ConfigureFormatters` which now able to configure `Default` formatter to be used.
- **sample:** add sample for MessagePack formatter - see `MessagePackIntegrationTest` and `MessagePackMediaTypeFormatter`.

### Deprecated code

- **http client builder:** `WithFormatters` has been marked as obsolete, instead the newly `ConfigureFormatters`.

## [1.2.1](https://github.com/sketch7/FluentlyHttpClient/compare/1.2.0...1.2.1) (2018-02-16)

### Bug Fixes

- **regex extensions:** `ReplaceTokens` now validates object instead of throwing null reference exception.
  This now will give better errors when using interpolations without value.
- **request builder:** `WithQueryParams` now omits query param when value is null instead of null reference exception.

### Deprecated code

- **collection extensions:** `Set` has been marked as obsolete, as it can be replaced by 1 liner.

## [1.2.0](https://github.com/sketch7/FluentlyHttpClient/compare/1.1.0...1.2.0) (2018-01-12)

### Features

- **graphql:** add support for GraphQL to have simpler api's specifically for GraphQL. (thanks to [Kurt Cassar](https://github.com/nismolo) for contribution)
- **consts:** add `XForwardedFor` in `HeaderTypes`.

New apis such as:

- `fluentHttpClient.CreateGqlRequest(query)` - Creates a new HTTP request and configure for GraphQL.
- `requestBuilder.AsGql(query)` - Configures an existing HTTP request builder as a GraphQL request.
- `requestBuilder.ReturnAsGqlResponse<T>()` - Sends request and unwrap GraphQL data to be available directly in the `.Data`.

See documentation for an example on how it can be used.

## [1.1.0](https://github.com/sketch7/FluentlyHttpClient/compare/1.0.0...1.1.0) (2017-07-02)

### Features

- **request builder:** implement `WithItem` which allows to set custom items that can be used to share data within the scope of request, response, and middleware.
- **request builder:** add `WithUserAgent` extension.
- **request builder:** add validation for request when 'GET' and has body content, to not be allowed. As it will blow up the underlying HttpClient.

- **request:** add `Formatters` which can be useful for middleware.
- **message state:** extract interface `IFluentHttpMessageState`, which both `FluentHttpRequest` and `FluentHttpResponse` implements.
  This will allow sharing implementations for extensions methods across `FluentHttpRequest` and `FluentHttpResponse` related to `Items`.

- **header builder:** extract interface `IFluentHttpHeaderBuilder`, which both `FluentHttpClientBuilder` and `FluentHttpRequestBuilder` implements.
  This will allow sharing implementations for extensions methods across `FluentHttpClientBuilder` and `FluentHttpRequestBuilder` related to `Headers`.

- **http client builder:** formatter JSON is now configured with camelcase property names by default.
- **http client builder:** now shares request builder headers extensions such as `WithUserAgent` and `WithBearerAuthentication`.
- **http client builder:** implement `ConfigureDefaults` which enables to configure defaults for newly created http clients.

- **logger middleware:** add extension method `UseLogging`.

- **consts:** add constants for headers and auth schemes `HeaderTypes` and `AuthSchemeTypes`

## [1.0.0](https://github.com/sketch7/FluentlyHttpClient/compare/0.3.0...1.0.0) (2017-06-30)

### Features

- **http client factory:** `Remove` now disposes `IFluentHttpClient`.
- **http client builder:** implement `WithFormatters` to be able to configure formatters.

- **http client:** implement `IDisposable` in order to dispose underlying `HttpClient`.

- **request builder:** implement `ReturnAsString`, `ReturnAsStream` and `ReturnAsByteArray`.

- **request:** `Method` and `Uri` has now also setters.
- **request:** add `Items` in order to share state across requests/response.

- **response:** `StatusCode` and `ReasonPhrase` has now also setters.
- **response:** `Items` now are shared with request.
- **response:** expose `Content` from `Message`.

- **middleware:**now supports arguments via `UseMiddleware<T>(args)`.

- **timer middleware:** now supports options, for configure `WarnThreshold`.
- **timer middleware:** add extension method `UseTimer` for convience.

### Code Refactoring

- **http client builder:** rename `AddMiddleware` to `UseMiddleware`.
- **http client builder:** `UseMiddleware<T>`, <T> is now constrained with `IFluentHttpMiddleware`.

- **request:** rename `Url` to `Uri`.

### BREAKING CHANGES

- `FluentHttpClientBuilder.AddMiddleware` has been renamed to `FluentHttpClientBuilder.UseMiddleware`.
- `FluentHttpClientBuilder.UseMiddleware` is now constrained with `IFluentHttpMiddleware`.
- `FluentHttpRequest.Url` has been renamed to `FluentHttpRequest.Uri`.
- `FluentHttpRequest` rename `RawRequest` to `Message`
- `FluentHttpResponse` rename `RawResponse` to `Message`

## [0.3.0](https://github.com/sketch7/FluentlyHttpClient/compare/0.2.1...0.3.0) (2017-06-28)

### Features

- **http client factory:** validate options on `Add`, in order to have better errors.
- **http client factory:** add default options to reduce the amount of config required (when not specified).
- **http client factory:** `Add` now returns newly created `FluentHttpClient`.
- **http client factory:** implement `WithRequestBuilderDefaults` additional hook in order customize `FluentHttpRequestBuilder` on creation.
- **http client factory:** implement `WithMessageHandler` in order to replace the HTTP stack for the HTTP client.
- **http client factory:** implement `Add` with `FluentHttpClientOptions`.
- **http client factory:** extract interface `IFluentHttpClientFactory` from `FluentHttpClientFactory`.

- **http client builder:** add new method `AddMiddleware(Type)`.
- **http client builder:** align naming with request builder.

- **request builder:** implement `WithSuccessStatus` to specify or not whether to throw or not when request is not successful.
- **request builder:** implement `WithCancellationToken` to be able to cancel http request.
- **request builder:** Http method now defaults as `Get`.

- **http client:** implement `Patch` and `Delete` methods.
- **http client:** extract interface `IFluentHttpClient` from `FluentHttpClient`.

### BREAKING CHANGES

- `IFluentHttpResponse` has been removed and added `FluentHttpResponse` instead. In addition, most of `FluentHttpResponse<T>` method has been changed with `FluentHttpResponse`.
  Most of the changes should only effect the internals, apart from middlewares.
- Now requests created by `FluentHttpRequestBuilder` won't throw unless specified when request is not succeeded.
- `FluentHttpClientFactory.Add` now returns the new `FluentHttpClient` instance instead of `FluentHttpClientFactory`.
- `FluentHttpClientBuilder` methods naming alignment
  - `SetBaseUrl` => `WithBaseUrl`
  - `SetTimeout` => `WithTimeout`
  - `AddHeader` => `WithHeader`
  - `AddHeaders` => `WithHeaders`
  - `AddHeaders` => `WithHeaders`
- Middleware related has been moved to `FluentlyHttpClient.Middleware` namespace, this effects:
  - `LoggerHttpMiddleware`
  - `TimerHttpMiddleware`
  - `IFluentHttpMiddleware`
  - `IFluentHttpMiddlewareRunner`
- `FluentHttpClientFactory` usages now is changed with `IFluentHttpClientFactory`
- `FluentHttpClient` usages now is changed with `IFluentHttpClient`

## [0.2.1](https://github.com/sketch7/FluentlyHttpClient/compare/0.2.0...0.2.1) (2017-06-25)

### Features

- **request builder:** validate request on `Build`, in order to have better errors.
- **request builder:** implement query string `WithQueryParams`.
- **request builder:** implement headers `WithHeader`, `WithHeaders` and `WithBearerAuthentication`.

- **http client:** implement `Send` method with `FluentHttpRequest`.

- **http client builder:** implement `AddHeaders` which accepts a dictionary.
- **http client builder:** `AddHeader` now replaces, instead of throwing when already defined.
