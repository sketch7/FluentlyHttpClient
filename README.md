[projectUri]: https://github.com/sketch7/FluentlyHttpClient
[projectGit]: https://github.com/sketch7/FluentlyHttpClient.git
[changeLog]: ./CHANGELOG.md

# Fluently Http Client
[![CircleCI](https://circleci.com/gh/sketch7/FluentlyHttpClient.svg?style=shield)](https://circleci.com/gh/sketch7/FluentlyHttpClient)
[![NuGet version](https://badge.fury.io/nu/fluentlyhttpclient.svg)](https://badge.fury.io/nu/fluentlyhttpclient)

Http Client for .NET Standard with fluent APIs which are intuitive, easy to use and also highly extensible.

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
Available for [.NET Standard 1.4+](https://docs.microsoft.com/en-gb/dotnet/standard/net-standard)

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
Add services via `.AddFluentlyHttpClient()`.

```cs
// using Startup.cs (can be elsewhere)
public void ConfigureServices(IServiceCollection services)
{
    services.AddFluentlyHttpClient();
}
```

Configure an Http client using the Http Factory (you need at least one).
```cs
// using Startup.cs (can be elsewhere)
public void Configure(IApplicationBuilder app, IFluentHttpClientFactory fluentHttpClientFactory)
{
  fluentHttpClientFactory.CreateBuilder(identifier: "platform") // keep a note of the identifier, its needed later
    .WithBaseUrl("http://sketch7.com") // required
    .WithHeader("user-agent", "slabs-testify")
    .WithTimeout(5)
    .UseMiddleware<LoggerHttpMiddleware>()
    .Register(); // register client builder to factory
}
```

### Basic usage

#### Simple API
Simple API (non-fluent) is good for simple requests as it has a clean, minimal API.

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
Fluent request API (request builder) allows to create more complex requests and provides further control on response.

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

### Fluent Http Client Builder
Http client builder is used to configure http clients in a fluent way.

#### Register to Factory

```cs
var clientBuilder = fluentHttpClientFactory.CreateBuilder(identifier: "platform")
    .WithBaseUrl("http://sketch7.com");
fluentHttpClientFactory.Add(clientBuilder);

// or similarly via the builder itself.
clientBuilder.Register().
```

#### Register multiple + share
There are multiple ways how to register multiple http clients. The following is a nice way of doing it:

```cs
fluentHttpClientFactory.CreateBuilder("platform")
    // shared
    .WithHeader("user-agent", "slabs-testify")
    .WithTimeout(5)
    .UseTimer()
    .UseMiddleware<LoggerHttpMiddleware>()

    // platform
    .WithBaseUrl("https://platform.com")
    .Register()

    // big-data - reuse all above and replace the below
    .Withdentifier("big-data")
    .WithBaseUrl("https://api.big-data.com")
    .Register();
```

#### Http Client Builder extra goodies

```cs
// message handler - set HTTP handler stack to use for sending requests
var mockHttp = new MockHttpMessageHandler();
httpClientBuilder.WithMessageHandler(mockHttp);

// request builder defaults - handler to customize defaults for request builder
httpClientBuilder.WithRequestBuilderDefaults(builder => builder.AsPut());

// formatters - used for content negotiation, for "Accept" and body media formats. e.g. JSON, XML, etc...
httpClientBuilder.WithFormatters(formatter => formatter.Add(new CustomFormatter()));
```

### Request builder
Request builder is used to build http requests in a fluent way.

#### Usage

```cs
LoginResponse loginResponse =
  await fluentHttpClient.CreateRequest("/api/auth/login")
    .AsPost() // set as HTTP Post
    .WithBody(new
    {
        Username = "test",
        Password = "test"
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

### Re-using Http Client from Factory
As a best practice rather than using a string each time for the identifier, it's better
to create an extension method for it.

```cs
public static class FluentHttpClientFactoryExtensions
{
    public static IFluentHttpClient GetPlatformClient(this IFluentHttpClientFactory factory)
        => factory.Get("platform");
}
```

### Middleware
Middleware are used to intercept request/response to add additional logic, or alter request/response.

Implementing a middleware for the HTTP client is quite straight forward, and its very similar to
ASP.NET Core MVC middleware.

These are provided out of the box:

| Middleware | Description                                   |
|------------|-----------------------------------------------|
| Timer      | Determine how long (timespan) requests takes. |
| Logger     | Log request/response.                         |

Two important points to keep in mind:
 - First argument within constructor has to be `FluentHttpRequestDelegate` which is generally called `next`.
 - During `Invoke` the `await _next(request);` must be invoked and return the response, in order to continue the flow.

 The following is the timer middleware implementation *(bit simplified)*.

```cs
public class TimerHttpMiddleware : IFluentHttpMiddleware
{
    private readonly FluentHttpRequestDelegate _next;
    private readonly TimerHttpMiddlewareOptions _options;
    private readonly ILogger _logger;

    public TimerHttpMiddleware(FluentHttpRequestDelegate next, TimerHttpMiddlewareOptions options, ILogger<TimerHttpMiddleware> logger)
    {
        _next = next;
        _options = options;
        _logger = logger;
    }

    public async Task<FluentHttpResponse> Invoke(FluentHttpRequest request)
    {
        var watch = Stopwatch.StartNew();
        var response = await _next(request); // this needs to be done to continue middleware flow
        var elapsed = watch.Elapsed;
        _logger.LogInformation("Executed request {request} in {timeTakenMillis}ms", request, elapsed.TotalMilliseconds);
        response.SetTimeTaken(elapsed);
        return response;
    }
}

namespace FluentlyHttpClient
{
    // Response extension methods - useful to extend FluentHttpResponse
    public static class TimerFluentResponseExtensions
    {
        private const string TimeTakenKey = "TIME_TAKEN";

        public static void SetTimeTaken(this FluentHttpResponse response, TimeSpan value)
          => response.Items.Add(TimeTakenKey, value);

        public static TimeSpan GetTimeTaken(this FluentHttpResponse response)
          => (TimeSpan)response.Items[TimeTakenKey];
    }

    // FluentHttpClientBuilder extension methods - add
    public static class FluentlyHttpMiddlwareExtensions
    {
        public static FluentHttpClientBuilder UseTimer(this FluentHttpClientBuilder builder, TimerHttpMiddlewareOptions options = null)
            => builder.UseMiddleware<TimerHttpMiddleware>(options ?? new TimerHttpMiddlewareOptions());
    }
}

// response extension usage
TimeSpan timeTaken = response.GetTimeTaken();
```

#### Middleware options
Options to middleware can be passed via an argument. Note it has to be the second argument within constructor.

```cs
                                                                       V - Options
public TimerHttpMiddleware(FluentHttpRequestDelegate next, TimerHttpMiddlewareOptions options, ILogger<TimerHttpMiddleware> logger)
```

Options can be passed when register a middleware.

#### Use a middleware

```cs
fluentHttpClientFactory.CreateBuilder("platform")
    .UseMiddleware<LoggerHttpMiddleware>() // register a middleware (without args)
    .UseMiddleware<TimerHttpMiddleware>(new TimerHttpMiddlewareOptions
      {
          WarnThreshold = TimeSpan.Zero
      }) // register a middleware with options (args)
    .UseTimer(new TimerHttpMiddlewareOptions
      {
          WarnThreshold = TimeSpan.Zero
      }) // register a middleware using extension method
```
As a best practice, it's best to provide an extension method for usage such as `UseTimer` 
especially when it has any arguments (options), as it won't be convenient to use.


#### Request/Response items
When using middleware additional data can be added to the request/response via the `.Items` of request/response,
in order to share state across middleware for the request or to extend response.

The timer middleware example is making use of it.

```cs
// set item
response.SetTimeTaken(elapsed);

// or similarly without extension method
response.Items.Add("TIME_TAKEN", value)

// get item
TimeSpan timeTaken = response.GetTimeTaken();

// or similarly without extension method
TimeSpan timeTaken = (TimeSpan)response.Items["TIME_TAKEN"];
```


### Extending
One of the key features is the ability to extend its own APIs easily.
In fact, several functions of the library itself are extensions, by using extension methods.

#### Extending Request Builder
An example of how can the request builder be extended.

```cs
public static class FluentHttpRequestBuilderExtensions
{
    public static FluentHttpRequestBuilder WithBearerAuthentication(this FluentHttpRequestBuilder builder, string token)
    {
        if (string.IsNullOrEmpty(token)) throw new ArgumentNullException(nameof(token));
        builder.WithHeader("Authorization", $"Bearer {token}");
        return builder;
    }
}
```

### Testing/Mocking
In order to test HTTP requests the library itself doesn't offer anything out of the box.
However we've been using [RichardSzalay.MockHttp](https://github.com/richardszalay/mockhttp), which we recommend.

#### Test example with RichardSzalay.MockHttp

```cs
[Fact]
public async void ShouldReturnContent()
{
    // build services
    var servicesProvider = new ServiceCollection()
      .AddFluentlyHttpClient()
      .AddLogging()
      .BuildServiceProvider();
    var fluentHttpClientFactory = servicesProvider.GetService<IFluentHttpClientFactory>();

    // define mocks
    var mockHttp = new MockHttpMessageHandler();
    mockHttp.When("https://sketch7.com/api/heroes/azmodan")
      .Respond("application/json", "{ 'name': 'Azmodan' }");

    fluentHttpClientFactory.CreateBuilder("platform")
      .WithBaseUrl("https://sketch7.com")
      .AddMiddleware<TimerHttpMiddleware>()
      .WithMessageHandler(mockHttp) // set message handler to mock
      .Register();

    var httpClient = fluentHttpClientFactory.Get("platform");
    var response = await httpClient.CreateRequest("/api/heroes/azmodan")
      .ReturnAsResponse<Hero>();

    Assert.NotNull(response.Data);
    Assert.Equal("Azmodan", response.Data.Name);
    Assert.NotEqual(TimeSpan.Zero, response.GetTimeTaken());
}
```

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