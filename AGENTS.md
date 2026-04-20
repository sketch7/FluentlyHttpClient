# FluentlyHttpClient — Agent Guide

Fluent HTTP client library for .NET 10 with middleware pipeline, GraphQL support, and response caching.
**NuGet packages**: `FluentlyHttpClient` (core) | `FluentlyHttpClient.Entity` (SQL-backed caching)

## Commands

```bash
dotnet restore                     # Restore dependencies
dotnet build                       # Build all projects
dotnet test                        # Run all tests (uses test.runsettings)
dotnet test --filter "FullyQualifiedName~SomeTest"  # Run specific tests
dotnet pack                        # Pack NuGet packages
dotnet run --project benchmark/FluentlyHttpClient.Benchmarks.csproj  # Benchmarks
```

## Project Layout

| Path                                     | Purpose                                                  |
| ---------------------------------------- | -------------------------------------------------------- |
| `src/FluentlyHttpClient/`                | Core library                                             |
| `src/FluentlyHttpClient.Entity/`         | EF Core + SQL Server response caching (optional package) |
| `test/`                                  | xUnit tests (unit + integration)                         |
| `benchmark/`                             | BenchmarkDotNet microbenchmarks                          |
| `samples/FluentlyHttpClient.Sample.Api/` | ASP.NET Core sample API used in integration tests        |

## Architecture

### Core Abstractions

- **`IFluentHttpClientFactory`** — singleton registry of named `IFluentHttpClient` instances
- **`FluentHttpClientBuilder`** — fluent builder for client config (base URL, headers, timeout, middleware)
- **`FluentHttpRequestBuilder`** — fluent builder for individual requests (method, URI template, query params, body)
- **`FluentHttpRequest`** / **`FluentHttpResponse`** / **`FluentHttpResponse<T>`** — wrappers around `HttpRequestMessage` / `HttpResponseMessage`
- **`IFluentHttpMiddleware`** — decorator pipeline interface; implement for cross-cutting concerns

### Middleware Pipeline

Middleware runs in registration order; `ActionExecuteMiddleware` is always the final stage (added internally).
Each middleware receives `FluentHttpMiddlewareContext` and calls `next(context)` to continue the chain.

```
[Logger] → [Timer] → [Cache] → [ActionExecuteMiddleware (sends HTTP)]
           ←──────────────────── response bubbles back ──────────────
```

Built-in middleware lives in `src/FluentlyHttpClient/Middleware/` and `src/FluentlyHttpClient/Caching/`.

### Request/Response Context

Both `FluentHttpRequest` and `FluentHttpResponse` implement `IFluentHttpMessageState`, which exposes an `Items` dictionary (`IDictionary<object, object>`).
Use `Items` to pass data between middleware stages — **do not use static/ambient state**.

### Response Caching

- Default: `MemoryResponseCacheService` (in-memory, registered by `AddFluentlyHttpClient()`)
- Optional: `RemoteResponseCacheService` (SQL Server via EF Core, registered by `AddFluentlyHttpClientEntity()`)
- Cache keys are derived from `request.GetHash()` — see `src/FluentlyHttpClient/RequestHashingExtensions.cs`

### DI Registration

```csharp
// Core — registers IFluentHttpClientFactory, IResponseCacheService (in-memory), IHttpResponseSerializer
services.AddFluentlyHttpClient();

// Entity (SQL caching) — replaces in-memory IResponseCacheService
services.AddFluentlyHttpClientEntity(connectionString);
```

## Conventions

- **C# coding style**: see [.github/instructions/csharp.instructions.md](.github/instructions/csharp.instructions.md)
- **Test conventions**: see [.github/instructions/tests.instructions.md](.github/instructions/tests.instructions.md)
- **Usage examples and API docs**: see [README.md](README.md)
- **Planned work**: see [docs/TODO.md](docs/TODO.md)

## Key Patterns

- All builders return `this` — every method participates in a fluent chain
- Extend request/client builder via extension methods on `FluentHttpRequestBuilder` / `FluentHttpClientBuilder` (see `*Extensions.cs` files)
- Add custom headers at client level (default for all requests) or at request level (single request)
- URI templates use `{param}` interpolation via `.WithInterpolationData(new { param = value })`
- GraphQL: use `FluentHttpClientGqlExtensions` in `src/FluentlyHttpClient/GraphQL/`
- File uploads: use `MultipartFormDataContentExtensions` in `src/FluentlyHttpClient/MultipartFormDataContentExtensions.cs`

## Testing

Tests use **xUnit + Shouldly + RichardSzalay.MockHttp**.
`ServiceTestUtil.cs` and `FluentlyTestExtensions.cs` provide shared test helpers.
Integration tests live in `test/Integration/` and reference the sample API project.

## Entity Sub-Project

`FluentlyHttpClient.Entity` adds persistent SQL-backed HTTP response caching.
Requires running EF Core migrations: `dotnet ef migrations add <Name> --project src/FluentlyHttpClient.Entity`.
Call `.Initialize()` on `FluentHttpClientDbContext` during app startup to apply migrations idempotently.
