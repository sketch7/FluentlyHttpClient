# Fluently Http Changelog

## [*vNext*](https://github.com/sketch7/FluentlyHttpClient/compare/0.2.1...0.3.0) (x)


## [0.2.1](https://github.com/sketch7/FluentlyHttpClient/compare/0.2.0...0.2.1) (2017-06-25)

### Features
- **request builder:** validate request on `Build`, in order to have better errors.
- **request builder:** implement query string `WithQueryParams`.
- **request builder:** implement headers `WithHeader`, `WithHeaders` and `WithBearerAuthentication`.

- **http client:** implement `Send` method with `FluentHttpRequest`.

- **http client builder:** implement `AddHeaders` which accepts a dictionary.
- **http client builder:** `AddHeader` now replaces, instead of throwing when already defined.