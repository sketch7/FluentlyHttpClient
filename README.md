[projectUri]: https://github.com/sketch7/FluentlyHttpClient
[projectGit]: https://github.com/sketch7/FluentlyHttpClient.git
[changeLog]: ./CHANGELOG.md

# Fluently Http Client
[![CircleCI](https://circleci.com/gh/sketch7/FluentlyHttpClient.svg?style=shield)](https://circleci.com/gh/sketch7/FluentlyHttpClient)
[![NuGet version](https://badge.fury.io/nu/fluentlyhttpclient.svg)](https://badge.fury.io/nu/fluentlyhttpclient)

Http Client for .NET Standard with fluent APIs which are intuitive, easy to use and also highly extensible.

*NOTE: This project is under development and is not intended for general production use yet.*

**Quick links**

[Change logs][changeLog] | [Project Repository][projectUri]

## Features
 - Fluent APIs
 - Middleware Support
   - Custom Classes with DI enabled
   - Access to both Request/Response within same scope (similar to MVC middleware)
   - Logger and Timer middleware out of the box
 - Multiple HttpClient support with a Fluent API for Client builder
 - Customizable Formatters (JSON, XML out of the box)
 - Url interpolation and query params e.g. person/{id} / person?id=1
 - Highly extensible

## Installation

### nuget
```
PM> Install-Package FluentlyHttpClient
```

### csproj

```xml
<PackageReference Include="FluentlyHttpClient" Version="*" />
```

## Usage

### Configure

Add services via `.AddFluentlyHttpClient()`
```cs
// using startup
public void ConfigureServices(IServiceCollection services)
{
  services.AddFluentlyHttpClient();
}
```

Configure a client using the Http Factory (you need at least one).
```cs
// using startup
public void Configure(IApplicationBuilder app, IFluentHttpClientFactory fluentHttpClientFactory)
{
  // keep a note of the identifier, its needed later
  fluentHttpClientFactory.CreateBuilder(identifier: "platform")
    .WithBaseUrl("http://sketch7.com") // required
    .WithHeader("user-agent", "slabs-testify")
    .WithTimeout(5)
    .AddMiddleware<LoggerHttpMiddleware>()
    .Register(); // register client builder to factory
}
```

### Basic usage

#### Simple API
Using the simple API (non fluent) is good for simple calls, as it has compact API.

```cs
// inject factory and get client
var httpClient = fluentHttpClientFactory.Get(identifier: "platform");

// HTTP GET + deserialize result (non-fleunt API)
Hero hero = await httpClient.Get<Hero>("/api/heroes/azmodan");

// HTTP POST + deserialize result (non-fleunt API)
Hero hero = await httpClient.Post<Hero>("/api/heroes/azmodan", new
{
  Title = "Lord of Sin"
});
```

#### Fluent Request API
Fluent request API allows to create more complex request and further control on response.

```cs
// inject factory and get client
var httpClient = fluentHttpClientFactory.Get(identifier: "platform");

// HTTP GET + return response and deserialize result (fluent API)
FluentHttpResponse<Hero> response = 
  await httpClient.CreateRequest("/api/heroes/azmodan")
    .ReturnAsResponse<Hero>(); // return with response

// HTTP POST + return response and deserialize result (fluent API)
Hero hero = await httpClient.CreateRequest("/api/heroes/azmodan")
    .AsPost()
    .WithBody(new
    {
      Title = "Lord of Sin"
    })
    .Return<Hero>(); // return deserialized result directly
```

### Using fluent http client builder
Http client builder is used to configure http client in a fluent way.

#### Register to factory

```cs
var clientBuilder = fluentHttpClientFactory.CreateBuilder(identifier: "platform")
    .WithBaseUrl("http://sketch7.com");
fluentHttpClientFactory.Add(clientBuilder);

// or similarly via the builder itself.
clientBuilder.Register().
```

#### Register multiple + share

There are multiple ways how to register multiple clients, this is a nice way to do it.
```cs
fluentHttpClientFactory.CreateBuilder("platform")
  // shared
  .WithHeader("user-agent", "slabs-testify")
  .WithTimeout(5)
  .AddMiddleware<TimerHttpMiddleware>()
  .AddMiddleware<LoggerHttpMiddleware>()

  // platform
  .WithBaseUrl("https://platform.com")
  .Register()

  // big-data - reuse all above and replace the below
  .Withdentifier("big-data")
  .WithBaseUrl("https://api.big-data.com")
  .Register();
```

#### http client builder goodies

```cs
// message handler - set HTTP handler stack to use for sending requests
var mockHttp = new MockHttpMessageHandler();
httpClientBuilder.WithMessageHandler(mockHttp);

// request builder defaults - handler to customize defaults for request builder
httpClientBuilder.WithRequestBuilderDefaults(builder => builder.AsPut());
```

### Using request builder
Request builder is used to configure http client in a fluent way.

#### Usage

```cs
LoginResponse loginResponse =
  await fluentHttpClient.CreateRequest("/api/auth/login")
    .AsPost() // set as HTTP Post
    .WithBody(new
    {
      username = "test",
      password = "test"
    }) // serialize body content
    .WithSuccessStatus() // ensure response success status
    .Return<LoginResponse>(); // send, deserialize result and return result directly.
```

#### Query params
```cs
requestBuilder.WithQueryParams(new 
{
  Take = 5,
  Filter = "warrior"
}); // => /url?filter=warrior&take=5
```

#### Interpolate Url
```cs
requestBuilder.WithUri("{Language}/heroes/{Hero}", new
  {
     Language = "en",
     Hero = "azmodan"
  }); // => /en/heroes/azmodan
```

#### ReturnAsReponse, ReturnAsResponse`<T>` and Return`<T>`

```cs
// send and returns HTTP response
FluentHttpResponse response = requestBuilder.ReturnAsResponse();

// send and returns HTTP response + deserialize and return result via `.Data`
FluentHttpResponse<Hero> response = requestBuilder.ReturnAsResponse<Hero>();

// send and returns derserialized result directly
Hero hero = requestBuilder.Return<Hero>();
```

### Re-using http client from factory
As a best practice, rather than using a string each time for the identifier, its better to create
an extension method for it.

```cs
public static class FluentHttpClientFactoryExtensions
{
  public static IFluentHttpClient GetPlatformClient(this IFluentHttpClientFactory factory) 
    => factory.Get("platform");
}
```

### Middleware
Implementing a middleware for the HTTP client is quite straight forward, its very similar to 
ASP.NET Core MVC middleware.

These are provided out of the box:
- Timer Middleware - Determine how long request/response takes.
- Logger Middleware - Log request/response.

The following is the timer middleware implementation (a bit simplified).

```cs
public class TimerHttpMiddleware : IFluentHttpMiddleware
{
  private readonly FluentHttpRequestDelegate _next;
  private readonly ILogger _logger;

  public TimerHttpMiddleware(FluentHttpRequestDelegate next, ILogger<TimerHttpMiddleware> logger)
  {
    _next = next;
    _logger = logger;
  }

  public async Task<FluentHttpResponse> Invoke(FluentHttpRequest request)
  {
    var watch = Stopwatch.StartNew();
    var response = await _next(request); // continue middleware chain
    var elapsed = watch.Elapsed;
    _logger.LogInformation("Executed request {request} in {timeTakenMillis}ms", request, elapsed.TotalMilliseconds);
    response.SetTimeTaken(elapsed);
    return response;
  }
}

// Response Extension methods - useful to extend FluentHttpResponse
namespace FluentlyHttpClient
{
  public static class TimerFluentResponseExtensions
  {
    private const string TimeTakenKey = "TIME_TAKEN";

    public static void SetTimeTaken(this FluentHttpResponse response, TimeSpan value)
      => response.Items.Add(TimeTakenKey, value);

    public static TimeSpan GetTimeTaken(this FluentHttpResponse response)
      => (TimeSpan)response.Items[TimeTakenKey];
  }
}
```

### Extending
One of the key features is the ability to extend its own APIs easily.

*todo*

### Test/Mocking
*todo*


## Contributing

### Setup Machine for Development
Install/setup the following:

- NodeJS v8+
- Visual Studio Code or similar code editor
- Git + SourceTree, SmartGit or similar (optional)

 ### Commands

```bash
# run tests
npm test

# bump version
npm version minor --no-git-tag # major | minor | patch | prerelease

# nuget pack (only)
npm run pack

# nuget publish dev (pack + publish + clean)
npm run publish:dev
```
