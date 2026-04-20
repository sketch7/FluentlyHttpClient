# Testing Minimal APIs

## WebApplicationFactory Setup

```csharp
public class ApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;

    public ApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace real services with mocks
                services.AddScoped<IUserService, MockUserService>();

                // Replace database
                services.RemoveAll<DbContextOptions<AppDbContext>>();
                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase("TestDb"));
            });

            builder.ConfigureTestServices(services =>
            {
                // Configure test-specific services
            });
        }).CreateClient();
    }

    [Fact]
    public async Task CreateUser_ReturnsCreated()
    {
        var request = new { Email = "test@example.com", Name = "Test" };

        var response = await _client.PostAsJsonAsync("/api/users", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var user = await response.Content.ReadFromJsonAsync<UserResponse>();
        user!.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task CreateUser_InvalidEmail_Returns400()
    {
        var request = new { Email = "", Name = "Test" };

        var response = await _client.PostAsJsonAsync("/api/users", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem!.Errors.Should().ContainKey("Email");
    }
}
```

## Testing with Authentication

```csharp
public class AuthenticatedApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthenticatedApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                // Replace auth with test scheme
                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", null);
            });
        });
    }

    private HttpClient CreateAuthenticatedClient(string userId, params string[] roles)
    {
        return _factory.CreateClient();
        // TestAuthHandler reads claims from custom header
    }

    [Fact]
    public async Task AdminEndpoint_WithAdminRole_ReturnsOk()
    {
        var client = CreateAuthenticatedClient("user-1", "Admin");
        client.DefaultRequestHeaders.Add("X-Test-UserId", "user-1");
        client.DefaultRequestHeaders.Add("X-Test-Roles", "Admin");

        var response = await client.GetAsync("/api/admin/settings");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AdminEndpoint_WithoutRole_Returns403()
    {
        var client = CreateAuthenticatedClient("user-1");
        client.DefaultRequestHeaders.Add("X-Test-UserId", "user-1");

        var response = await client.GetAsync("/api/admin/settings");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}

// Test auth handler
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var userId = Request.Headers["X-Test-UserId"].FirstOrDefault();
        if (string.IsNullOrEmpty(userId))
            return Task.FromResult(AuthenticateResult.NoResult());

        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId) };

        var roles = Request.Headers["X-Test-Roles"].FirstOrDefault()?.Split(',') ?? [];
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r.Trim())));

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
```

## Testing Database Operations

```csharp
public class DatabaseTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private AsyncServiceScope _scope;
    private AppDbContext _db = null!;

    public DatabaseTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<AppDbContext>>();
                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase($"TestDb-{Guid.NewGuid()}"));
            });
        });
    }

    public async Task InitializeAsync()
    {
        _scope = _factory.Services.CreateAsyncScope();
        _db = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await _db.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _db.Database.EnsureDeletedAsync();
        await _scope.DisposeAsync();
    }

    [Fact]
    public async Task GetUser_ExistingUser_ReturnsUser()
    {
        // Arrange
        var user = new User { Email = "test@example.com", Name = "Test" };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/users/{user.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<UserResponse>();
        result!.Email.Should().Be("test@example.com");
    }
}
```

## Testing Endpoint Filters

```csharp
[Fact]
public async Task ValidationFilter_InvalidRequest_Returns422()
{
    var client = _factory.CreateClient();

    var invalidOrder = new { CustomerId = 0, Items = Array.Empty<object>() };

    var response = await client.PostAsJsonAsync("/api/orders", invalidOrder);

    response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
    problem!.Errors.Should().NotBeEmpty();
}
```

## Test Project Setup

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="10.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="10.0.0" />
    <PackageReference Include="FluentAssertions" Version="7.0.0" />
    <PackageReference Include="Moq" Version="4.20.0" />
    <PackageReference Include="xunit" Version="2.9.0" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\src\MyApi\MyApi.csproj" />
  </ItemGroup>
</Project>
```

## Integration Test Base Class

```csharp
public abstract class IntegrationTestBase : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    protected readonly HttpClient Client;
    protected readonly IServiceProvider Services;

    protected IntegrationTestBase(WebApplicationFactory<Program> factory)
    {
        var customFactory = factory.WithWebHostBuilder(ConfigureWebHost);
        Client = customFactory.CreateClient();
        Services = customFactory.Services;
    }

    protected virtual void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Override services for all tests
        });
    }

    public virtual Task InitializeAsync() => Task.CompletedTask;
    public virtual Task DisposeAsync() => Task.CompletedTask;

    protected async Task<T> GetAsync<T>(string url)
    {
        var response = await Client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<T>())!;
    }

    protected async Task<HttpResponseMessage> PostAsync<T>(string url, T content)
    {
        return await Client.PostAsJsonAsync(url, content);
    }
}
```
