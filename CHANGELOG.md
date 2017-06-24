# Fluently Http Changelog

## [*vNext*](https://github.com/sketch7/FluentlyHttpClient/compare/0.2.0...0.3.0) (x)

### Features
- **request builder:** validating request on `Build`, in order to have better errors.
- **request builder:** implement query string `WithQueryParams`.
- **request builder:** implement headers `WithHeader`, `WithHeaders` and `WithBearerAuthentication`.

- **http client:** implement `Send` method with `FluentHttpRequest`.

- **http client builder:** implement `AddHeaders`
- **http client builder:** `AddHeader` now replaces instead of throwing when already defined.