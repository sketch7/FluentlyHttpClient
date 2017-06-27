# Fluently Http Changelog

 [*vNext*](https://github.com/sketch7/FluentlyHttpClient/compare/0.3.0...0.4.0) (2017-?-?)

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