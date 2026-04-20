# Key Libraries Reference

## MediatR - CQRS / Mediator Pattern

```csharp
// Installation
// dotnet add package MediatR

// Registration
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Command
public record CreateOrderCommand(int CustomerId, List<OrderItem> Items) : IRequest<OrderResult>;

// Handler
public class CreateOrderHandler(AppDbContext db) : IRequestHandler<CreateOrderCommand, OrderResult>
{
    public async Task<OrderResult> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        var order = new Order { CustomerId = request.CustomerId };
        db.Orders.Add(order);
        await db.SaveChangesAsync(ct);
        return new OrderResult(order.Id);
    }
}

// Usage in endpoint
app.MapPost("/orders", async (CreateOrderCommand cmd, IMediator mediator) =>
{
    var result = await mediator.Send(cmd);
    return TypedResults.Created($"/orders/{result.Id}", result);
});

// Pipeline behavior (cross-cutting)
public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        logger.LogInformation("Handling {RequestName}", typeof(TRequest).Name);
        var response = await next();
        logger.LogInformation("Handled {RequestName}", typeof(TRequest).Name);
        return response;
    }
}
```

**Docs:** [MediatR Wiki](https://github.com/jbogard/MediatR/wiki)

---

## FluentValidation

```csharp
// Installation
// dotnet add package FluentValidation.DependencyInjectionExtensions

// Registration
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Validator
public class CreateOrderValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.CustomerId).GreaterThan(0);
        RuleFor(x => x.Items).NotEmpty().WithMessage("Order must have items");
        RuleForEach(x => x.Items).SetValidator(new OrderItemValidator());
    }
}

// With MediatR pipeline
public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var context = new ValidationContext<TRequest>(request);
        var failures = validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
            throw new ValidationException(failures);

        return await next();
    }
}
```

**Docs:** [FluentValidation Documentation](https://docs.fluentvalidation.net/en/latest/)

---

## Mapster - Object Mapping

```csharp
// Installation
// dotnet add package Mapster
// dotnet add package Mapster.DependencyInjection

// Registration
builder.Services.AddMapster();

// Simple mapping
var dto = entity.Adapt<UserDto>();

// With configuration
TypeAdapterConfig<User, UserDto>
    .NewConfig()
    .Map(dest => dest.FullName, src => $"{src.FirstName} {src.LastName}")
    .Ignore(dest => dest.Password);

// Projection in EF Core
var users = await db.Users
    .ProjectToType<UserDto>()
    .ToListAsync();

// Interface-based (for DI)
public class UserService(IMapper mapper)
{
    public UserDto Map(User user) => mapper.Map<UserDto>(user);
}
```

**Docs:** [Mapster Wiki](https://github.com/MapsterMapper/Mapster/wiki)

---

## ErrorOr - Result Pattern

```csharp
// Installation
// dotnet add package ErrorOr

// Define errors
public static class UserErrors
{
    public static Error NotFound(int id) => Error.NotFound("User.NotFound", $"User {id} not found");
    public static Error DuplicateEmail => Error.Conflict("User.DuplicateEmail", "Email already exists");
}

// Service returning ErrorOr
public async Task<ErrorOr<User>> GetUserAsync(int id)
{
    var user = await _db.Users.FindAsync(id);
    return user is null
        ? UserErrors.NotFound(id)
        : user;
}

// Endpoint handling
app.MapGet("/users/{id}", async (int id, IUserService svc) =>
{
    var result = await svc.GetUserAsync(id);
    return result.Match(
        user => TypedResults.Ok(user),
        errors => errors.First().Type switch
        {
            ErrorType.NotFound => TypedResults.NotFound(),
            ErrorType.Conflict => TypedResults.Conflict(),
            ErrorType.Validation => TypedResults.BadRequest(errors),
            _ => TypedResults.Problem()
        }
    );
});

// Chaining operations
public async Task<ErrorOr<OrderConfirmation>> CreateOrderAsync(CreateOrderRequest request)
{
    return await ValidateRequest(request)
        .ThenAsync(req => CreateOrder(req))
        .ThenAsync(order => SendConfirmation(order));
}
```

**Docs:** [ErrorOr README](https://github.com/amantinband/error-or)

---

## Polly - Resilience

```csharp
// Installation
// dotnet add package Microsoft.Extensions.Http.Resilience

// Already covered in infrastructure.md, here's advanced usage:

// Custom resilience pipeline
builder.Services.AddResiliencePipeline("custom", builder =>
{
    builder
        .AddRetry(new RetryStrategyOptions
        {
            MaxRetryAttempts = 3,
            Delay = TimeSpan.FromSeconds(1),
            BackoffType = DelayBackoffType.Exponential,
            ShouldHandle = new PredicateBuilder().Handle<HttpRequestException>()
        })
        .AddCircuitBreaker(new CircuitBreakerStrategyOptions
        {
            FailureRatio = 0.5,
            SamplingDuration = TimeSpan.FromSeconds(10),
            MinimumThroughput = 10,
            BreakDuration = TimeSpan.FromSeconds(30)
        })
        .AddTimeout(TimeSpan.FromSeconds(10));
});

// Usage
public class MyService(ResiliencePipelineProvider<string> pipelineProvider)
{
    public async Task<string> GetDataAsync()
    {
        var pipeline = pipelineProvider.GetPipeline("custom");
        return await pipeline.ExecuteAsync(async ct =>
        {
            // Your operation here
            return await FetchDataAsync(ct);
        });
    }
}
```

**Docs:** [Polly Documentation](https://www.pollydocs.org/)

---

## Serilog - Structured Logging

```csharp
// Installation
// dotnet add package Serilog.AspNetCore
// dotnet add package Serilog.Sinks.Console
// dotnet add package Serilog.Sinks.Seq

// Configuration
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithCorrelationId()
    .WriteTo.Console(new CompactJsonFormatter())
    .WriteTo.Seq("http://localhost:5341")
    .CreateLogger();

builder.Host.UseSerilog();

// Request logging
app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (ctx, httpCtx) =>
    {
        ctx.Set("UserId", httpCtx.User.FindFirstValue("sub"));
    };
});

// Structured logging
logger.LogInformation("Order {OrderId} created for {CustomerId} with {ItemCount} items",
    order.Id, order.CustomerId, order.Items.Count);
```

**Docs:** [Serilog Wiki](https://github.com/serilog/serilog/wiki)

---

## .NET Aspire - Cloud-Native Orchestration

```csharp
// AppHost/Program.cs
var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .AddDatabase("mydb");

var redis = builder.AddRedis("redis");

var api = builder.AddProject<Projects.MyApi>("api")
    .WithReference(postgres)
    .WithReference(redis);

builder.AddProject<Projects.MyFrontend>("frontend")
    .WithReference(api);

builder.Build().Run();

// In API project
builder.AddServiceDefaults(); // Adds health checks, telemetry, etc.

var postgres = builder.AddNpgsqlDataSource("mydb");
var redis = builder.AddRedisClient("redis");
```

**Docs:** [.NET Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/get-started/aspire-overview)

---

## Quick Reference Table

| Library | Purpose | NuGet Package |
|---------|---------|---------------|
| MediatR | CQRS, mediator | `MediatR` |
| FluentValidation | Validation rules | `FluentValidation.DependencyInjectionExtensions` |
| Mapster | Object mapping | `Mapster.DependencyInjection` |
| ErrorOr | Result pattern | `ErrorOr` |
| Polly | Resilience | `Microsoft.Extensions.Http.Resilience` |
| Serilog | Structured logging | `Serilog.AspNetCore` |
| Aspire | Cloud orchestration | `Aspire.Hosting` |
| Refit | Type-safe HTTP | `Refit.HttpClientFactory` |
| Bogus | Fake data | `Bogus` |
| Humanizer | String manipulation | `Humanizer` |
