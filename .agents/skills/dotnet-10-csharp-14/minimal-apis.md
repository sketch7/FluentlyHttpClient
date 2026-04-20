# Minimal API Patterns

## Built-in Validation (.NET 10) - PREFERRED

**PREFER .NET 10's built-in validation over FluentValidation for simple cases.**

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddValidation(); // One line enables automatic validation!

var app = builder.Build();

// DTO with DataAnnotations
public record CreateUserDto(
    [Required, EmailAddress] string Email,
    [Required, StringLength(100, MinimumLength = 2)] string Name,
    [Range(0, 150)] int? Age
);

// Endpoint - validation happens automatically
app.MapPost("/users", (CreateUserDto user) => TypedResults.Ok(user));
// Returns 400 with validation errors automatically

// Opt out for internal endpoints
app.MapPost("/internal", (InternalDto dto) => TypedResults.Ok())
    .DisableValidation();
```

**When to use FluentValidation instead:**
- Complex cross-property validation
- Async validation (database uniqueness checks)
- Conditional validation rules
- Custom error message formatting

---

## TypedResults (MANDATORY - Not Optional)

**NEVER use `Results.Ok()`. ALWAYS use `TypedResults.Ok()`.**

The difference is critical for OpenAPI documentation:

❌ **WRONG** (No OpenAPI metadata):
```csharp
app.MapGet("/users/{id}", async (int id, IUserService svc) =>
{
    var user = await svc.GetAsync(id);
    return user is not null ? Results.Ok(user) : Results.NotFound();
});
// OpenAPI doesn't know the response type!
```

✅ **CORRECT** (Full OpenAPI metadata):
```csharp
app.MapGet("/users/{id}", async (int id, IUserService svc) =>
{
    var user = await svc.GetAsync(id);
    return user is not null
        ? TypedResults.Ok(user)
        : TypedResults.NotFound();
})
.WithName("GetUser")
.WithOpenApi();
// OpenAPI correctly shows UserDto as 200 response
```

### Multiple Return Types

```csharp
// Explicit return type documents ALL possible responses
app.MapPost("/users", async Task<Results<Created<UserResponse>, ValidationProblem, Conflict>>
    (CreateUserDto dto, IUserService svc, CancellationToken ct) =>
{
    if (await svc.EmailExistsAsync(dto.Email, ct))
        return TypedResults.Conflict();

    var user = await svc.CreateAsync(dto, ct);
    return TypedResults.Created($"/api/users/{user.Id}", new UserResponse(user));
});
```

### TypedResults Quick Reference

| Method | Status | Use When |
|--------|--------|----------|
| `TypedResults.Ok(data)` | 200 | Successful GET/PUT |
| `TypedResults.Created(uri, data)` | 201 | Successful POST |
| `TypedResults.Accepted()` | 202 | Async processing started |
| `TypedResults.NoContent()` | 204 | Successful DELETE |
| `TypedResults.NotFound()` | 404 | Resource not found |
| `TypedResults.BadRequest()` | 400 | Invalid request |
| `TypedResults.Conflict()` | 409 | Resource conflict |
| `TypedResults.ValidationProblem(errors)` | 400 | Validation failed |
| `TypedResults.Problem()` | 500 | Server error |

## Endpoint Filters

```csharp
// Reusable validation filter
public class ValidationFilter<T> : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext ctx,
        EndpointFilterDelegate next)
    {
        var validator = ctx.HttpContext.RequestServices
            .GetService<IValidator<T>>();
        if (validator is null) return await next(ctx);

        var arg = ctx.Arguments.OfType<T>().FirstOrDefault();
        if (arg is null) return await next(ctx);

        var result = await validator.ValidateAsync(arg);
        return result.IsValid
            ? await next(ctx)
            : TypedResults.ValidationProblem(result.ToDictionary());
    }
}

app.MapPost("/orders", Handler)
    .AddEndpointFilter<ValidationFilter<CreateOrderDto>>();
```

## Server-Sent Events (.NET 10)

```csharp
app.MapGet("/events", (CancellationToken ct) =>
{
    async IAsyncEnumerable<SseEvent> GenerateEvents()
    {
        while (!ct.IsCancellationRequested)
        {
            yield return new SseEvent { Data = DateTime.Now.ToString() };
            await Task.Delay(1000, ct);
        }
    }
    return TypedResults.ServerSentEvents(GenerateEvents());
});
```

---

# Modular Monolith / Feature Folders

## Structure

```
src/
├── Features/
│   ├── Users/
│   │   ├── UsersModule.cs        # DI + endpoints registration
│   │   ├── Endpoints/
│   │   │   ├── CreateUser.cs     # Handler + Request + Response
│   │   │   └── GetUser.cs
│   │   ├── Services/
│   │   └── Data/
│   ├── Orders/
│   │   └── OrdersModule.cs
│   └── Shared/                   # Cross-cutting concerns only
├── Program.cs
```

## Module Registration Pattern

```csharp
// Features/Users/UsersModule.cs
public static class UsersModule
{
    public static IServiceCollection AddUsersModule(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IUserRepository, UserRepository>();
        return services;
    }

    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users")
            .RequireAuthorization();

        group.MapGet("/{id}", GetUser.Handle);
        group.MapPost("/", CreateUser.Handle);
        return app;
    }
}

// Program.cs
builder.Services
    .AddUsersModule()
    .AddOrdersModule();

app.MapUsersEndpoints()
   .MapOrdersEndpoints();
```

## Vertical Slice Handler

```csharp
// Features/Users/Endpoints/CreateUser.cs
public static class CreateUser
{
    public record Request(string Email, string Name);
    public record Response(Guid Id, string Email);

    public static async Task<Results<Created<Response>, ValidationProblem>> Handle(
        Request request,
        IUserService service,
        CancellationToken ct)
    {
        var user = await service.CreateAsync(request, ct);
        return TypedResults.Created($"/api/users/{user.Id}",
            new Response(user.Id, user.Email));
    }
}
```

## Module Communication Rules

- Modules communicate via public APIs or events only
- Never access another module's database directly
- Shared kernel for cross-cutting only (not business logic)
- Use MassTransit/MediatR for inter-module events

```csharp
// Good: Public API call
var user = await _usersApi.GetUserAsync(userId);

// Good: Event-based
await _mediator.Publish(new OrderCreatedEvent(order.Id));

// BAD: Direct DB access to another module
var user = await _otherModuleDbContext.Users.FindAsync(userId);
```
