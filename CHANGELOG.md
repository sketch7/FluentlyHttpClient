# Fluently Http Changelog

 [*vNext*](https://github.com/sketch7/FluentlyHttpClient/compare/1.2.0...1.3.0) (201X-X-X)
 
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