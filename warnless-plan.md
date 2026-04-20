# Warnless Plan — FluentlyHttpClient

Goal: eliminate **all** compiler and analyzer warnings across every project in the solution, then lock in `TreatWarningsAsErrors=true`.

---

## 1. Warning Inventory

> **Total unique warnings: 123**
> (deduplicated by file + line; raw build output doubles each warning due to MSBuild dependency re-evaluation)

### 1.1 Currently suppressed (NoWarn in Directory.Build.props)

| Code             | Count (approx) | Description                              |
| ---------------- | -------------- | ---------------------------------------- |
| CS1591           | ~80–100        | Missing XML doc comment on public member |
| CS1701/1702/1705 | ~5             | Assembly version binding redirects       |

> CS1591 is suppressed globally. It is **not** counted in the 123 above and must be tackled separately once the active warnings are cleared.

---

### 1.2 Active warnings (123 unique)

| Code          |   Total | src/core | src/Entity |  test/ | benchmark/ | samples/ | Description                                         |
| ------------- | ------: | -------: | ---------: | -----: | ---------: | -------: | --------------------------------------------------- |
| **CS8602**    |  **53** |        6 |          1 |     40 |          6 |        — | Dereference of possibly null reference              |
| **xUnit1048** |  **18** |        — |          — |     18 |          — |        — | `async void` test — must convert to `async Task`    |
| **CS8604**    |  **11** |        5 |          — |      6 |          — |        — | Possible null reference argument                    |
| **CS8618**    |   **9** |        2 |          — |      4 |          — |        3 | Non-nullable property must be non-null at ctor exit |
| **CS8601**    |   **7** |        7 |          — |      — |          — |        — | Possible null reference assignment                  |
| **CS8619**    |   **6** |        3 |          2 |      — |          1 |        — | Nullability mismatch on target type                 |
| **CS8603**    |   **6** |        3 |          1 |      2 |          — |        — | Possible null reference return                      |
| **CS8620**    |   **4** |        2 |          2 |      — |          — |        — | Argument nullability mismatch                       |
| **CS0618**    |   **4** |        3 |          — |      1 |          — |        — | Obsolete API: `HttpRequestMessage.Properties`       |
| **CS8625**    |   **2** |        — |          — |      2 |          — |        — | Cannot convert null literal to non-nullable type    |
| **CS8600**    |   **2** |        2 |          — |      — |          — |        — | Converting null/possible-null to non-nullable       |
| **xUnit1012** |   **1** |        — |          — |      1 |          — |        — | Null used for non-nullable `[InlineData]` parameter |
| **Total**     | **123** |   **33** |      **6** | **73** |      **7** |    **3** |                                                     |

---

## 2. Risk & Auto-Fix Assessment

| Code                     | Regression Risk |   `dotnet format` auto-fix    | Notes                                                                                                                                                                                                                               |
| ------------------------ | :-------------: | :---------------------------: | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| xUnit1048                |     **LOW**     | No (no code fixer registered) | Test-only; `async void → async Task` is safe mechanical change                                                                                                                                                                      |
| xUnit1012                |     **LOW**     |              No               | Test-only; fix `[InlineData(null, …)]` parameter type                                                                                                                                                                               |
| CS8618 (test/samples)    |     **LOW**     | No (IDE quick-fix available)  | Add `required` or convert to `record`; test/sample models only                                                                                                                                                                      |
| CS8618 (src/core)        |     **MED**     |              No               | `GqlRequest.Query` and `FluentHttpClient.BaseUrl` are public API — adding `required` is a **breaking change** to callers who use object-initializer without `required`. Prefer nullable `string?` annotation or proper constructor. |
| CS8602 (test/benchmark)  |     **LOW**     |              No               | Null-forgiving `!` operator or null guards inside test code; no public API impact                                                                                                                                                   |
| CS8602 (src/core)        |     **MED**     |              No               | Null guards in middleware/caching/headers can change runtime behavior                                                                                                                                                               |
| CS8604 (test)            |     **LOW**     |              No               | Null-forgiving in test assertions; no prod impact                                                                                                                                                                                   |
| CS8604 (src/core)        |     **MED**     |              No               | Fixing argument nullability can require guard clauses and throw/return                                                                                                                                                              |
| CS8601 (src/core)        |     **MED**     |              No               | Null-assignment guards; affects caching/headers state                                                                                                                                                                               |
| CS8603 (src/core+Entity) |     **MED**     |              No               | Return type annotations; may require nullable return types on public methods                                                                                                                                                        |
| CS8619 (src/core+Entity) |     **MED**     |              No               | Type annotation fixes; EF Core Entity changes require migration testing                                                                                                                                                             |
| CS8620 (src/core+Entity) |     **MED**     |              No               | Nullability covariance fixes; EF converter changes need DB layer testing                                                                                                                                                            |
| CS0618 (src/core)        |     **MED**     |              No               | Migrating `HttpRequestMessage.Properties` → `.Options` changes request-item propagation; regression test required                                                                                                                   |
| CS0618 (test)            |     **LOW**     |              No               | Test-only usage; safe to update once src/ is fixed                                                                                                                                                                                  |
| CS8625 (test)            |     **LOW**     |              No               | Test-specific null literals; fix parameter type to nullable                                                                                                                                                                         |
| CS8600 (src/core)        |     **LOW**     |              No               | Local variable null conversion; add null-check                                                                                                                                                                                      |
| CS1591 (suppressed)      |     **LOW**     |              No               | Pure documentation; zero runtime risk                                                                                                                                                                                               |

### Auto-fix summary

`dotnet format --diagnostics` does **not** produce code fixes for any of the above warning codes. All fixes are either:
- **Manual edits** — most null-safety fixes
- **IDE Quick Actions** — Roslyn code-fix suggestions (VS / C# Dev Kit) can apply many of these in bulk via "Fix all in document / project"

---

## 3. Staged Fix Plan

Priority rules applied:
- High count + trivial → **first**
- Trivial auto-fixable (or nearly so) combined → single stage
- Low count / complex / higher regression risk → **last**

### Stage 1 — xUnit async void (18 + 1 = 19 warnings, test-only) `LOW risk`

**Codes**: `xUnit1048` (18), `xUnit1012` (1)
**Scope**: `test/` only
**Files affected**:
- `test/FluentHttpClientFactoryTest.cs` — 1× xUnit1048
- `test/FluentHttpClientTest.cs` — 2× xUnit1048
- `test/TimerHttpMiddlewareTest.cs` — 2× xUnit1048
- `test/FluentHttpRequestBuilderTest.cs` — 4× xUnit1048, 1× xUnit1012
- `test/LoggingHttpMiddlewareTest.cs` — 3× xUnit1048
- `test/HttpMiddlewareTest.cs` — 4× xUnit1048
- `test/Integration/SerilogIntegrationTest.cs` — 1× xUnit1048
- `test/Integration/LoadTest.cs` — 1× xUnit1048

**Fix strategy**:
- Change every `public async void TestName()` → `public async Task TestName()`
- For `xUnit1012`: change `[InlineData(null, …)]` parameter type to `string?`
- Use IDE "Fix all xUnit1048 in project" quick action, or a `sed`/find-replace

**Validation**: `dotnet test` — all tests pass

---

### Stage 2 — CS8618: Non-nullable property initialisation (test + samples, 7 warnings) `LOW risk`

**Codes**: `CS8618`
**Scope**: `test/Integration/LoadTest.cs`, `samples/FluentlyHttpClient.Sample.Api/Heroes/Hero.cs`
**Files affected**:
- `test/Integration/LoadTest.cs` — 4× (response model DTO)
- `samples/Heroes/Hero.cs` — 3×

**Fix strategy**:
- Convert plain classes with uninitialized string properties to `record` types or add `required` modifier + initializer
- For `LoadTest` DTOs: convert to `record` with `init` setters (matches instruction: `record` for immutable data models)
- For `Hero.cs`: same pattern

**Validation**: `dotnet build` — 0 warnings in these files; run integration tests

---

### Stage 3 — CS0618: Obsolete API migration (4 warnings) `MED risk`

**Codes**: `CS0618`
**Scope**: `src/FluentlyHttpClient/HttpMessageExtensions.cs`, `test/HttpMiddlewareTest.cs`
**Files affected**:
- `src/FluentlyHttpClient/HttpMessageExtensions.cs` — 3× (`HttpRequestMessage.Properties`)
- `test/HttpMiddlewareTest.cs` — 1×

**Fix strategy**:
- Replace `request.Properties` with `request.Options` throughout `HttpMessageExtensions.cs`
- `HttpRequestMessage.Options` is `HttpRequestOptions` (`IDictionary<string, object?>`) vs `.Properties` which was `IDictionary<string, object>`
- Update the test to match new API
- Ensure request item propagation (`WithItem`/`GetItem`) still works end-to-end

**Validation**: Full `dotnet test` — integration + unit; focus on middleware pipeline tests

---

### Stage 4 — CS8618: Non-nullable src/core properties (2 warnings) `MED risk`

**Codes**: `CS8618`
**Scope**: `src/FluentlyHttpClient/`
**Files affected**:
- `src/FluentlyHttpClient/GraphQL/GqlRequest.cs` — `Query` property
- `src/FluentlyHttpClient/FluentHttpClient.cs` — `BaseUrl` property

**Fix strategy**:
- `GqlRequest.Query`: convert to `record` with `required string Query { get; init; }` — this is a public API **breaking change**. Add to CHANGELOG.
- `FluentHttpClient.BaseUrl`: evaluate whether `required` or private-setter constructor assignment is appropriate. Since this is set via builder, a proper constructor that enforces it is safer.

**Validation**: `dotnet build`; run all tests; check sample API compiles; update README/CHANGELOG

---

### Stage 5 — CS8602/CS8604/CS8625 in tests & benchmark (47 + 6 + 2 = ~49 warnings) `LOW risk`

**Codes**: `CS8602` (test/benchmark, 47), `CS8604` (test, 6), `CS8625` (test, 2)
**Scope**: `test/`, `benchmark/`

**Files with high CS8602 density**:
- `test/FluentHttpRequestBuilderTest.cs` — ~20
- `test/Integration/ResponseCacheIntegrationTest.cs` — 6
- `test/Integration/FileUploadIntegrationTest.cs` — 3
- `test/Integration/MessagePackIntegrationTest.cs` — 2
- `test/LoggingHttpMiddlewareTest.cs` — 3
- `test/HttpResponseSerializerTest.cs` — 2
- `test/FluentHttpClientFactoryTest.cs` — 3
- `benchmark/Benchmarking.cs` — 6+

**Fix strategy**:
- In tests/benchmark, null-dereferences on `.Data`, `.Items[]`, assertion return values are expected to be non-null after a successful HTTP response. Use null-forgiving `result!` or introduce `ShouldNotBeNull()` assertion before accessing members.
- For `CS8625` null literals in `[InlineData]`: make the parameter `string?`
- For `CS8604` in test assertions (`Assert.Single(null_collection)`): add explicit null check or use `ShouldNotBeNull()`

**Validation**: `dotnet test` — all tests pass

---

### Stage 6 — CS8600/CS8601/CS8603/CS8602 in src/core (25 warnings) `MED risk`

**Codes**: `CS8600` (2), `CS8601` (7), `CS8602` (6), `CS8603` (3)
**Scope**: `src/FluentlyHttpClient/`
**Files affected**:
- `src/FluentlyHttpClient/FluentHttpHeaders.cs` — 6× CS8601 + 1× CS8602
- `src/FluentlyHttpClient/RequestTracker.cs` — 1× CS8603 + 1× CS8601
- `src/FluentlyHttpClient/RequestHashingExtensions.cs` — 1× CS8602 + 1× CS8600
- `src/FluentlyHttpClient/Middleware/FluentMiddlewareHttpHandler.cs` — 1× CS8604 + 1× CS8603
- `src/FluentlyHttpClient/Caching/ResponseCacheMiddleware.cs` — 1× CS8602
- `src/FluentlyHttpClient/FluentHttpResponse.cs` — 1× CS8602
- `src/FluentlyHttpClient/Utils/ObjectExtensions.cs` — 2× CS8604 + 1× CS8619
- `src/FluentlyHttpClient/Utils/RegexExtensions.cs` — 1× CS8603
- `src/FluentlyHttpClient/HttpMessageExtensions.cs` — residual after Stage 3
- `src/FluentlyHttpClient/Caching/ResponseCacheService.cs` — 2× CS8619

**Fix strategy**:
- `FluentHttpHeaders`: null-returning dictionary indexers — fix by adjusting type annotations (`string[]?` vs `string[]`) and null-coalescing where values are assigned
- `RequestTracker`: Add null check on `_tracker.TryGetValue()`
- `RequestHashingExtensions`: Add null-guard on body stream
- `FluentMiddlewareHttpHandler`: Fix return type to `Task<FluentHttpResponse?>` or add throw on null
- `ResponseCacheService`: Fix generic return type annotation (`Task<FluentHttpResponse?>`)
- `ObjectExtensions.ToDictionary`: Fix to `IDictionary<string, object?>` properly
- `RegexExtensions.Match`: Fix return type to `string?`

**Validation**: Full `dotnet test` — all unit + integration tests pass

---

### Stage 7 — CS8604/CS8619/CS8620/CS8602 in src/Entity (6 warnings) `MED risk`

**Codes**: `CS8620` (2), `CS8602` (1), `CS8603` (1)
**Scope**: `src/FluentlyHttpClient.Entity/`
**Files affected**:
- `src/FluentlyHttpClient.Entity/HttpResponseMapping.cs` — 2× CS8620 (EF Core ValueConverter nullability)
- `src/FluentlyHttpClient.Entity/RemoteResponseCacheService.cs` — 1× CS8602
- `src/FluentlyHttpClient.Entity/DataSerializer.cs` — 1× CS8603

**Fix strategy**:
- `HttpResponseMapping.cs`: Fix `ValueConverter<FluentHttpHeaders, string>` to `ValueConverter<FluentHttpHeaders?, string>` so EF Core nullability constraint matches
- `RemoteResponseCacheService.cs`: Add null guard on cached entry
- `DataSerializer.cs`: Fix return type to `string?`

**Validation**: `dotnet build` Entity project; run Entity integration tests if available

---

### Stage 8 — CS1591: Missing XML doc comments (suppressed, ~80–100 warnings) `LOW risk`

**Codes**: `CS1591`
**Scope**: `src/FluentlyHttpClient/` and `src/FluentlyHttpClient.Entity/` (all public API surface)

**Fix strategy**:
- Remove `1591` from `NoWarn` in `Directory.Build.props`
- Use IDE "Fix all CS1591" quick action or add XML doc stubs to every public member
- Focus on: `IFluentHttpClientFactory`, `FluentHttpClientBuilder`, `FluentHttpRequestBuilder`, middleware interfaces, extensions
- Per coding conventions: XML docs required on all public members

**Validation**: `dotnet build` — 0 warnings

---

### Stage 9 — Harden: enable TreatWarningsAsErrors `N/A`

**After all stages complete:**
1. Remove all remaining `NoWarn` entries except for legitimate suppressions with comments
2. Remove `#pragma warning disable` blocks in `Exceptions.cs` (fix underlying CS1591)
3. Remove `#pragma warning disable 618` in `test/Utils/QueryStringUtilsTest.cs` (fix underlying API)
4. Set `TreatWarningsAsErrors=true` in `Directory.Build.props`
5. Verify full `dotnet build` and `dotnet test` pass with zero warnings

---

## 4. Stage Summary (Priority Order)

| Stage | Codes                        | Count | Area         | Risk  | Auto-fix | Complexity  |
| ----- | ---------------------------- | ----: | ------------ | :---: | :------: | :---------: |
| 1     | xUnit1048, xUnit1012         |    19 | test         |  LOW  | ✗ (IDE)  |   trivial   |
| 2     | CS8618                       |     7 | test/samples |  LOW  | ✗ (IDE)  |   trivial   |
| 3     | CS0618                       |     4 | src/test     |  MED  |    ✗     |   medium    |
| 4     | CS8618                       |     2 | src          |  MED  |    ✗     |   medium    |
| 5     | CS8602/CS8604/CS8625         |   ~55 | test/bench   |  LOW  |    ✗     |   trivial   |
| 6     | CS8600/CS8601/CS8602/CS8603  |   ~25 | src/core     |  MED  |    ✗     |   medium    |
| 7     | CS8602/CS8603/CS8619/CS8620  |     6 | src/Entity   |  MED  |    ✗     |   medium    |
| 8     | CS1591                       |   ~90 | src          |  LOW  |    ✗     | high volume |
| 9     | Harden TreatWarningsAsErrors |     — | all          |  N/A  |    —     |   config    |

---

## 5. Todo Checklist

### Stage 1 — xUnit async void

- [ ] `test/FluentHttpClientTest.cs` — change 2× `async void` → `async Task`
- [ ] `test/TimerHttpMiddlewareTest.cs` — change 2× `async void` → `async Task`
- [ ] `test/FluentHttpRequestBuilderTest.cs` — change 4× `async void` → `async Task`; fix `xUnit1012` `[InlineData]` null parameter → `string?`
- [ ] `test/LoggingHttpMiddlewareTest.cs` — change 3× `async void` → `async Task`
- [ ] `test/HttpMiddlewareTest.cs` — change 4× `async void` → `async Task`
- [ ] `test/FluentHttpClientFactoryTest.cs` — change 1× `async void` → `async Task`
- [ ] `test/Integration/SerilogIntegrationTest.cs` — change 1× `async void` → `async Task`
- [ ] `test/Integration/LoadTest.cs` — change 1× `async void` → `async Task`
- [ ] Run `dotnet test` — verify all pass

### Stage 2 — CS8618 test/samples models

- [ ] `test/Integration/LoadTest.cs` — convert response DTOs to `record` with `required` init properties
- [ ] `samples/FluentlyHttpClient.Sample.Api/Heroes/Hero.cs` — convert to `record` with `required` init properties
- [ ] Run `dotnet build` — verify 0 CS8618 in test/samples
- [ ] Run `dotnet test` — verify all pass

### Stage 3 — CS0618 obsolete API

- [ ] `src/FluentlyHttpClient/HttpMessageExtensions.cs` — migrate `request.Properties` → `request.Options`; update type usage
- [ ] `test/HttpMiddlewareTest.cs` — update test that accesses `.Properties` to use `.Options`
- [ ] Run `dotnet test` — verify middleware pipeline tests pass

### Stage 4 — CS8618 src/core public API

- [ ] `src/FluentlyHttpClient/GraphQL/GqlRequest.cs` — add `required` to `Query`; update CHANGELOG (breaking change)
- [ ] `src/FluentlyHttpClient/FluentHttpClient.cs` — fix `BaseUrl` null-safety (constructor enforcement or annotation)
- [ ] Run `dotnet build` + `dotnet test`

### Stage 5 — CS8602/CS8604/CS8625 test/benchmark

- [ ] `test/FluentHttpRequestBuilderTest.cs` — add `ShouldNotBeNull()` assertions or `!` for response `.Data` accesses
- [ ] `test/Integration/ResponseCacheIntegrationTest.cs` — fix 6× CS8602
- [ ] `test/Integration/FileUploadIntegrationTest.cs` — fix 3× CS8602
- [ ] `test/Integration/MessagePackIntegrationTest.cs` — fix 2× CS8602
- [ ] `test/LoggingHttpMiddlewareTest.cs` — fix 3× CS8602
- [ ] `test/HttpResponseSerializerTest.cs` — fix 2× CS8602
- [ ] `test/FluentHttpClientFactoryTest.cs` — fix CS8602 + CS8604 (`Assert.Single` on possible-null)
- [ ] `test/FluentHttpHeadersTest.cs` — fix CS8602 + CS8604
- [ ] `benchmark/Benchmarking.cs` — fix 6× CS8602 + CS8619
- [ ] `test/Utils/QueryStringUtilsTest.cs` — fix CS8603; remove existing `#pragma warning disable 618`
- [ ] Run `dotnet test` — verify all pass

### Stage 6 — Null safety in src/core

- [ ] `src/FluentlyHttpClient/FluentHttpHeaders.cs` — fix 6× CS8601 + 1× CS8602 + 1× CS8620 + 1× CS8602 (null assignments in indexer setters)
- [ ] `src/FluentlyHttpClient/RequestTracker.cs` — fix CS8603 + CS8601 (null-safe `TryGetValue`)
- [ ] `src/FluentlyHttpClient/RequestHashingExtensions.cs` — fix CS8602 + CS8600 (null-guard on body)
- [ ] `src/FluentlyHttpClient/Middleware/FluentMiddlewareHttpHandler.cs` — fix CS8604 + CS8603 (return type / null guard)
- [ ] `src/FluentlyHttpClient/Caching/ResponseCacheMiddleware.cs` — fix CS8602 (null guard on cached response)
- [ ] `src/FluentlyHttpClient/Caching/ResponseCacheService.cs` — fix 2× CS8619 (nullable return type annotation)
- [ ] `src/FluentlyHttpClient/FluentHttpResponse.cs` — fix CS8602 (null-guard on message property)
- [ ] `src/FluentlyHttpClient/Utils/ObjectExtensions.cs` — fix CS8604 + CS8619 (dictionary type annotation)
- [ ] `src/FluentlyHttpClient/Utils/RegexExtensions.cs` — fix CS8603 (return `string?`)
- [ ] `src/FluentlyHttpClient/HttpMessageExtensions.cs` — fix residual CS8602/CS8604/CS8600 after Stage 3
- [ ] Run `dotnet test` — full suite including integration

### Stage 7 — Null safety in src/Entity

- [ ] `src/FluentlyHttpClient.Entity/HttpResponseMapping.cs` — fix 2× CS8620 (ValueConverter nullability for EF Core)
- [ ] `src/FluentlyHttpClient.Entity/RemoteResponseCacheService.cs` — fix CS8602 (null guard)
- [ ] `src/FluentlyHttpClient.Entity/DataSerializer.cs` — fix CS8603 (return type `string?`)
- [ ] Run `dotnet build` Entity project; run integration tests if available

### Stage 8 — XML documentation (CS1591)

- [ ] Remove `1591` from `NoWarn` in `Directory.Build.props`
- [ ] Run `dotnet build` to discover all CS1591 locations
- [ ] Add XML doc comments to all public types/members in `src/FluentlyHttpClient/`
- [ ] Add XML doc comments to all public types/members in `src/FluentlyHttpClient.Entity/`
- [ ] Remove `#pragma warning restore/disable CS1591` blocks in `src/FluentlyHttpClient/Exceptions.cs`
- [ ] Run `dotnet build` — 0 CS1591

### Stage 9 — Harden

- [ ] Remove all `NoWarn` entries in `Directory.Build.props` (or keep only `1701;1702;1705` if still needed)
- [ ] Set `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` in `Directory.Build.props`
- [ ] Run `dotnet build` — 0 warnings, 0 errors
- [ ] Run `dotnet test` — all pass
- [ ] Open PR: "chore: TreatWarningsAsErrors — warnless"
