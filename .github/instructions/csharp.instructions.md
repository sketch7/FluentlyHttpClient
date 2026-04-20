---
description: "C# coding conventions for this project. Apply when writing or reviewing any C# code: formatting, naming, nullability, language features, guard clauses, and extension patterns."
applyTo: "**/*.cs"
---

# C# Coding Conventions

## Language & Target

- Target `net10.0`; use C# 14 features (`<LangVersion>latest</LangVersion>`)
- Nullable reference types enabled globally — no `!` suppressions without an explanatory comment; use `??` throw patterns
- Never leave warnings; do not use `#pragma warning disable` except for deliberate namespace placement (see [Extension Method Namespaces](#extension-method-namespaces))
- XML docs required on all public members

## Formatting

### Method Chaining

Each chained call on its own line, indented one level. Lambda bodies follow the same rule — short lambdas inline, longer ones indented:

```csharp
builder.Services
    .AddMultitenancy<AppTenant>(opts => opts
        .WithHttpResolver<AppTenant, AppTenantHttpResolver>()
        .WithTenants(tenantRegistry.GetAll())
        .WithServices(tsb => tsb
            .For(t => t.Organization == OrganizationNames.Riot, s => s
                .AddScoped<IHeroDataClient, MockLoLHeroDataClient>()
            )
            .For(t => t.Organization == OrganizationNames.Blizzard, s => s
                .AddScoped<IHeroDataClient, MockHotsHeroDataClient>()
            )
        )
    );
```

### Constructor Parameters

Each parameter on its own line; closing `)` on its own line. Attributes for a parameter go on the line immediately before it:

```csharp
public HeroGrain(
    IServiceScopeFactory scopeFactory,
    [PersistentState("heroes", "heroes")]
    IPersistentState<HeroGrainState> state
)
```

### Expression Bodies

For single-expression members: prefer expression bodies (`=>`). Place `=>` on the **next line** (indented) when the expression is long; keep it on the **same line** when short:

```csharp
// Long — arrow on next line
public Task<IReadOnlyList<Hero>> GetAllAsync()
    => Task.FromResult<IReadOnlyList<Hero>>(_state.State.Heroes);

// Short — same line when it fits within the line length
public string Key => _key;
```

## Naming & Types

- Generic tenant constraint: always `where TTenant : class, ITenant`
- Use `record` for immutable data/domain models
- Use `sealed class` for mutable stateful objects
- Apply `sealed` to all concrete implementations not intended for subclassing
- Use `init` setters on record properties; prefer object initializer syntax

## Guard Clauses / Early Exit

Prefer early returns over nested `if` blocks when it avoids nesting without duplicating logic:

```csharp
// ✅ Guard at top — happy path is flat
public void Process(string key)
{
    if (key is null) return;
    if (!IsValid(key)) return;

    DoWork(key);
}

// ❌ Unnecessary nesting
public void Process(string key)
{
    if (key is not null)
    {
        if (IsValid(key))
            DoWork(key);
    }
}
```

## Exception / Null Guards

```csharp
// Null or whitespace guard
ArgumentException.ThrowIfNullOrWhiteSpace(tenantKey);

// Null-coalescing throw
public AppTenant Get(string key) =>
    GetOrDefault(key) ?? throw new KeyNotFoundException($"Tenant '{key}' not found.");
```

## Extension Method Namespaces

Place extension methods in the namespace of the **extended type**, not the containing project's namespace, so consumers discover them without extra `using` directives:

```csharp
// ✅ Correct — file lives in Sketch7.Multitenancy.AspNet but uses the extended type's namespace
#pragma warning disable IDE0130 // Namespace intentionally matches extended type not folder
namespace Sketch7.Multitenancy;

public static class AspNetMultitenancyBuilderExtensions
{
    extension<TTenant>(MultitenancyBuilder<TTenant> builder) where TTenant : class, ITenant { ... }
}

// ❌ Wrong — follows folder structure, forces an extra using on callers
namespace Sketch7.Multitenancy.AspNet;
```

> Extensions on framework/third-party types (`IServiceCollection`, `IApplicationBuilder`, `ISiloBuilder`) use the **package's own namespace** — we cannot reuse Microsoft's.

## C# 14 Extension Blocks

Always use C# 14 `extension(...)` blocks instead of traditional `this` parameter methods:

```csharp
// ✅ Correct
public static class MyExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddMyService<TImpl>() where TImpl : class
        {
            services.AddScoped<TImpl>();
            return services;
        }
    }
}

// ❌ Wrong
public static IServiceCollection AddMyService<TImpl>(this IServiceCollection services) { ... }
```

> **Known SDK limitation (10.0.x):** Extension blocks with both a generic receiver (`extension<T>(Builder<T> b)`) and method-level generic parameters do not resolve. Fall back to a traditional `this` extension method in that case.

## Fluent Builders

Builder methods always return `this` for chaining.
