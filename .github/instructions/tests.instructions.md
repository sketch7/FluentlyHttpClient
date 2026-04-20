---
description: "Use when writing, reviewing, or generating tests. Covers xUnit + Shouldly patterns, tenant scope wiring, test doubles, integration test setup, and naming conventions."
applyTo: "test/**/*.cs"
---

# Test Conventions

## Libraries

- **xUnit** — `[Fact]`, `[Theory]`, `[InlineData]`
- **Shouldly** — fluent assertions (`ShouldBe`, `ShouldBeOfType`, `ShouldThrow`)
- **Microsoft.AspNetCore.Mvc.Testing** — integration tests via `WebApplicationFactory<Program>`

## Naming

Pattern: `Subject_WhenCondition_ExpectedOutcome` or `Subject_Action_ExpectedOutcome`

```csharp
// ✅
public void WithServices_For_ProxyResolvesCorrectServiceForCurrentTenant()
public async Task Middleware_Returns400_WhenTenantNotResolved()

// ❌
public void TestServiceResolution()
```

## Structure

Follow Arrange / Act / Assert with section comments. Collapse to expression body for trivial single-assertion tests:

```csharp
[Fact]
public async Task Middleware_SetsTenantAccessor_WhenTenantResolved()
{
    // Arrange
    ...

    // Act
    ...

    // Assert
    ...
}

// Expression body — only when there is no setup and a single assertion
[Theory]
[InlineData("lol", "grain-1", "tenant/lol/grain-1")]
public void Create_ReturnsExpectedCompositeKey(string tenantKey, string grainKey, string expected)
    => TenantGrainKey.Create(tenantKey, grainKey).ShouldBe(expected);
```

## Avoid Boilerplate — Reusable Setup

When the same setup sequence appears in more than two tests, extract it. Prefer local factory helpers over test class fields for stateless setup, and `IClassFixture<T>` only when the fixture is genuinely expensive to create (e.g. `WebApplicationFactory`).

**Common extractions:**

```csharp
// Repeated: build provider + scope + set tenant → extract a helper
private static IServiceScope BuildScope(string tenantKey, Action<MultitenancyBuilder<TestTenant>>? configure = null)
{
    var services = new ServiceCollection();
    var builder = services.AddMultitenancy<TestTenant>();
    configure?.Invoke(builder);
    var provider = services.BuildServiceProvider();
    var scope = provider.CreateScope();
    scope.ServiceProvider.SetTenant(new TestTenant { Key = tenantKey });
    return scope;
}

// Usage — no repeated wiring per test
using var scope = BuildScope("tenant-a");
var service = scope.ServiceProvider.GetRequiredService<ITestService>();
```

For middleware tests, centralise `DefaultHttpContext` creation:

```csharp
private static DefaultHttpContext BuildHttpContext(IServiceProvider sp)
    => new() { RequestServices = sp, Response = { Body = new MemoryStream() } };
```

Do **not** extract when the setup variation _is_ the thing being tested — keep those inline so the test is self-contained.

## Scope & Tenant Wiring

Create a **fresh** `ServiceProvider` scope per test. Never share scopes or providers across tests. Set the tenant on `TenantAccessor<TTenant>` directly:

```csharp
var provider = services.BuildServiceProvider();

using var scope = provider.CreateScope();
scope.ServiceProvider.SetTenant(new TestTenant { Key = "tenant-a" });

var service = scope.ServiceProvider.GetRequiredService<ITestService>();
```

Use the `SetTenant` helper (defined in `TestHelpers.cs`) to avoid repeating the accessor lookup:

```csharp
// TestHelpers.cs
internal static class TestHelpers
{
    internal static IServiceProvider SetTenant<TTenant>(this IServiceProvider sp, TTenant tenant)
        where TTenant : class, ITenant
    {
        sp.GetRequiredService<TenantAccessor<TTenant>>().Tenant = tenant;
        return sp;
    }
}
```

## Assertions

```csharp
value.ShouldBe("expected");
value.ShouldBeOfType<ConcreteType>();
value.ShouldNotBeNull();
value.ShouldBeSameAs(otherRef);                         // reference equality
collection.ShouldContain(x => x.Key == "lol");

// Exceptions
Should.Throw<InvalidOperationException>(() => ...);
await Should.ThrowAsync<ArgumentException>(async () => ...);
```

## Async Tests

Always return `Task` — never `async void`:

```csharp
[Fact]
public async Task Something_DoesXyz() { ... }           // ✅

[Fact]
public async void Something_DoesXyz() { ... }           // ❌
```

## Test Doubles

Place at the end of the file after a `// ---- Test doubles ----` separator. Use `file` visibility when a double is only needed in one file; move shared doubles to `TestDoubles.cs`.

```csharp
// ---- Test doubles ----

public record TestTenant : ITenant
{
    public string Key { get; init; } = "test";
    public string Organization { get; init; } = string.Empty;
}

public interface ITestService { }
public sealed class TestServiceA : ITestService { }
public sealed class TestServiceB : ITestService { }

file sealed class StaticTenantResolver<TTenant> : ITenantHttpResolver<TTenant>
    where TTenant : class, ITenant
{
    private readonly TTenant? _tenant;

    public StaticTenantResolver(TTenant? tenant) => _tenant = tenant;

    public Task<TTenant?> Resolve(HttpContext context) => Task.FromResult(_tenant);
}
```

## Integration Tests

Use `IClassFixture<WebApplicationFactory<Program>>`. Set the tenant via request header (the middleware resolves it from the header, not DI):

```csharp
public class SampleApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public SampleApiIntegrationTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task GetTenant_Returns_LoL_Tenant_ForLolHeader()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-SSV-Tenant", "lol");

        var response = await client.GetAsync("/api/tenants/current");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var tenant = await response.Content.ReadFromJsonAsync<AppTenant>();
        tenant!.Key.ShouldBe("lol");
    }
}
```
