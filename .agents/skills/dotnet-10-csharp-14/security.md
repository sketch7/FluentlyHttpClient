# Security Patterns

## Authentication - JWT Bearer

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://your-identity-provider.com";
        options.Audience = "your-api-audience";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"))
    .AddPolicy("CanReadOrders", policy => policy.RequireClaim("scope", "orders:read"))
    .AddPolicy("PremiumUser", policy => policy
        .RequireAuthenticatedUser()
        .RequireClaim("subscription", "premium"));
```

## Securing Endpoints

```csharp
// Require authentication for entire group
var group = app.MapGroup("/api/orders")
    .RequireAuthorization();

// Specific policy
group.MapDelete("/{id}", DeleteOrder.Handle)
    .RequireAuthorization("AdminOnly");

// Allow anonymous for specific endpoint
app.MapGet("/api/health", () => "OK")
    .AllowAnonymous();

// Multiple policies (all must pass)
app.MapPost("/api/admin/settings", UpdateSettings.Handle)
    .RequireAuthorization("AdminOnly", "PremiumUser");
```

---

## CORS Configuration

```csharp
builder.Services.AddCors(options =>
{
    // Named policy for specific origins
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("https://app.example.com", "https://admin.example.com")
              .WithMethods("GET", "POST", "PUT", "DELETE")
              .WithHeaders("Content-Type", "Authorization")
              .AllowCredentials()
              .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    });

    // Development policy
    options.AddPolicy("Development", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Apply globally
app.UseCors("AllowFrontend");

// Or per-endpoint
app.MapGet("/api/public", GetPublicData)
    .RequireCors("AllowFrontend");
```

---

## Rate Limiting

```csharp
builder.Services.AddRateLimiter(options =>
{
    // Fixed window policy
    options.AddFixedWindowLimiter("fixed", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 100;
        opt.QueueLimit = 10;
    });

    // Sliding window per user
    options.AddSlidingWindowLimiter("perUser", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.SegmentsPerWindow = 6;
        opt.PermitLimit = 60;
    });

    // Token bucket for API
    options.AddTokenBucketLimiter("api", opt =>
    {
        opt.TokenLimit = 100;
        opt.ReplenishmentPeriod = TimeSpan.FromSeconds(10);
        opt.TokensPerPeriod = 10;
    });

    // Concurrency limiter
    options.AddConcurrencyLimiter("concurrent", opt =>
    {
        opt.PermitLimit = 10;
        opt.QueueLimit = 5;
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Custom response
    options.OnRejected = async (context, ct) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        await context.HttpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = 429,
            Title = "Too Many Requests",
            Detail = "Rate limit exceeded. Try again later."
        }, ct);
    };
});

app.UseRateLimiter();

// Apply to endpoints
app.MapGet("/api/search", Search.Handle)
    .RequireRateLimiting("perUser");

// Apply to group
app.MapGroup("/api/public")
    .RequireRateLimiting("fixed");
```

---

## OpenAPI Security Documentation

```csharp
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, ct) =>
    {
        // Add security scheme
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes = new Dictionary<string, OpenApiSecurityScheme>
        {
            ["Bearer"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "JWT Authorization header using the Bearer scheme."
            },
            ["ApiKey"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Header,
                Name = "X-API-Key",
                Description = "API Key authentication"
            }
        };

        // Apply security globally
        document.SecurityRequirements.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            }] = Array.Empty<string>()
        });

        return Task.CompletedTask;
    });
});

// Per-endpoint security documentation
app.MapGet("/api/admin/users", GetUsers.Handle)
    .WithOpenApi(op =>
    {
        op.Security = new List<OpenApiSecurityRequirement>
        {
            new()
            {
                [new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                }] = new[] { "admin:read" }
            }
        };
        return op;
    });
```

---

## Middleware Pipeline (Order Matters!)

```csharp
var app = builder.Build();

// 1. Exception handling (catch everything downstream)
app.UseExceptionHandler();
app.UseStatusCodePages();

// 2. HTTPS redirection
app.UseHttpsRedirection();

// 3. Static files (if any)
app.UseStaticFiles();

// 4. Routing (before auth)
app.UseRouting();

// 5. CORS (after routing, before auth)
app.UseCors();

// 6. Rate limiting
app.UseRateLimiter();

// 7. Authentication (identify user)
app.UseAuthentication();

// 8. Authorization (check permissions)
app.UseAuthorization();

// 9. Custom middleware
app.UseRequestLogging();

// 10. Endpoints
app.MapEndpoints();
```

**Critical rules:**
- `UseExceptionHandler` → first
- `UseAuthentication` → before `UseAuthorization`
- `UseRouting` → before auth middleware
- `UseCors` → before auth but after routing
