[projectUri]: https://github.com/sketch7/FluentlyHttpClient
[projectGit]: https://github.com/sketch7/FluentlyHttpClient.git
[changeLog]: ./CHANGELOG.md

# Fluently Http Client <!-- omit in toc -->
[![CI](https://github.com/sketch7/FluentlyHttpClient/actions/workflows/dotnet-publish.yml/badge.svg)](https://github.com/sketch7/FluentlyHttpClient/actions/workflows/dotnet-publish.yml)
[![NuGet version](https://badge.fury.io/nu/fluentlyhttpclient.svg)](https://badge.fury.io/nu/fluentlyhttpclient)

Http Client for .NET Standard with fluent APIs which are intuitive, easy to use and also highly extensible.

**Quick links**

[Change logs][changeLog] | [Project Repository][projectUri]

## Features
- Fluent APIs
- Highly extensible
- Middleware Support
  - Custom Classes with DI enabled
  - Access to both Request/Response within same scope (similar to ASPNET middleware)
  - Logger and Timer middleware out of the box
- Multiple HttpClient support with a Fluent API for Client builder
- Customizable Formatters (JSON, XML out of the box)
- Url interpolation and query params e.g. `person/{id}` / `person?id=1`
- GraphQL support
- File upload support

## Installation

## dotnet support

| Version | .NET               | Status                                                                                                                                                                                               |
| ------- | ------------------ | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1.x     | .NET Standard 1.4+ |                                                                                                                                                                                                      |
| 2.x     | .NET Standard 2    |                                                                                                                                                                                                      |
| 3.x     | .NET Standard 2    | [![CI](https://github.com/sketch7/FluentlyHttpClient/actions/workflows/dotnet-publish.yml/badge.svg?branch=3.x)](https://github.com/sketch7/FluentlyHttpClient/actions/workflows/dotnet-publish.yml) |
| 4.x     | net8               | [![CI](https://github.com/sketch7/FluentlyHttpClient/actions/workflows/dotnet-publish.yml/badge.svg?branch=4.x)](https://github.com/sketch7/FluentlyHttpClient/actions/workflows/dotnet-publish.yml) |

### NuGet
```
PM> Install-Package FluentlyHttpClient
```

### csproj

```xml
<PackageReference Include="FluentlyHttpClient" Version="*" />
```

## Table of Contents <!-- omit in toc -->
- [Features](#features)
- [Installation](#installation)
- [dotnet support](#dotnet-support)
  - [NuGet](#nuget)
  - [csproj](#csproj)
- [Usage](#usage)
  - [Configure](#configure)
  - [Basic usage](#basic-usage)
    - [Simple API](#simple-api)
    - [Fluent Request API](#fluent-request-api)
  - [Fluent Http Client Builder](#fluent-http-client-builder)
    - [Register to Factory](#register-to-factory)
    - [Register multiple + share](#register-multiple--share)
    - [Create Http Client from Client](#create-http-client-from-client)
    - [Configure defaults for Http Clients](#configure-defaults-for-http-clients)
    - [Http Client Builder extra goodies](#http-client-builder-extra-goodies)
    - [Re-using Http Client from Factory](#re-using-http-client-from-factory)
  - [Request Builder](#request-builder)
    - [Usage](#usage-1)
    - [Query params](#query-params)
    - [Interpolate Url](#interpolate-url)
    - [ReturnAsReponse, ReturnAsResponse`<T>` and Return`<T>`](#returnasreponse-returnasresponset-and-returnt)
  - [GraphQL](#graphql)
  - [Middleware](#middleware)
    - [Middleware options](#middleware-options)
    - [Use a middleware](#use-a-middleware)
    - [Request/Response items](#requestresponse-items)
  - [Extending](#extending)
    - [Extending Request Builder](#extending-request-builder)
    - [Extending Request Builder/Client Builder headers](#extending-request-builderclient-builder-headers)
    - [Extending Request/Response items](#extending-requestresponse-items)
  - [Recipes](#recipes)
    - [File upload](#file-upload)
    - [Simple Single file HttpClient](#simple-single-file-httpclient)
  - [Testing/Mocking](#testingmocking)
    - [Test example with RichardSzalay.MockHttp](#test-example-with-richardszalaymockhttp)
- [Contributing](#contributing)
  - [Setup Machine for Development](#setup-machine-for-development)
  - [Commands](#commands)

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
Fluent request API (request builder) allows to create more complex requests and provides further control on the response.

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

#### Create Http Client from Client
Its also possible to create a new http client from an http client, sort of sub-client which inherits options from its creator.
This might be good to pass defaults for a specific endpoint.

```cs
var httpClient = factory.Get("platform");
var paymentsClient = httpClient.CreateClient("payments")
  .WithHeader("X-Gateway", "xxx")
  .WithTimeout(30)
  .Build();
```

#### Configure defaults for Http Clients
Its also possible to configure builder defaults for all http clients via `ConfigureDefaults` within `IFluentHttpClientFactory`.
See example below.

```cs
fluentHttpClientFactory.ConfigureDefaults(builder
    => builder.WithUserAgent("sketch7")
        .WithTimeout(5)
);
```

#### Http Client Builder extra goodies

```cs
// message handler - set HTTP handler stack to use for sending requests
var mockHttp = new MockHttpMessageHandler();
httpClientBuilder.WithMessageHandler(mockHttp);

// request builder defaults - handler to customize defaults for request builder
httpClientBuilder.WithRequestBuilderDefaults(builder => builder.AsPut());

// formatters - used for content negotiation, for "Accept" and body media formats. e.g. JSON, XML, etc...
httpClientBuilder.ConfigureFormatters(opts =>
    {
      opts.Default = new MessagePackMediaTypeFormatter();
      opts.Formatters.Add(new CustomFormatter());
    });
```

#### Re-using Http Client from Factory
As a best practice rather than using a string each time for the identifier, it's better
to create an extension method for it.

```cs
public static class FluentHttpClientFactoryExtensions
{
    public static IFluentHttpClient GetPlatformClient(this IFluentHttpClientFactory factory)
        => factory.Get("platform");
}
```


### Request Builder
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
        Roles = new List<string> { "warrior", "assassin" },
    }, opts => {
        opts.CollectionMode = QueryStringCollectionMode.CommaSeparated;
        opts.KeyFormatter = key => key.ToLower();
    }); // => /url?roles=warrior,assassin&take=5
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


### GraphQL
FluentlyHttpClient :heart: GraphQL. First class support for GraphQL to be able to create request/response even simpler.

```cs
// configure globally to use uri for GraphQL endpoint.
httpClientBuilder.WithRequestBuilderDefaults(requestBuilder => requestBuilder.WithUri("api/graphql"));

// send and returns HTTP response + deserialize and return result via `.Data` directly
FluentHttpResponse<Hero> response =
  await fluentHttpClient.CreateGqlRequest("{ hero {name, title } }")
    .ReturnAsGqlResponse<Hero>();
    // => response.Data.Title
```


### Middleware
Middleware's are used to intercept request/response to add additional logic or alter request/response.

Implementing a middleware for the HTTP client is quite straight forward, and it's very similar to
ASP.NET Core middleware.

These are provided out of the box:

| Middleware | Description                                   |
| ---------- | --------------------------------------------- |
| Timer      | Determine how long (timespan) requests takes. |
| Logger     | Log request/response.                         |

Two important points to keep in mind:
 - The first argument within constructor has to be `FluentHttpMiddlewareDelegate` which is generally called `next`.
 - The second argument within constructor has to be `FluentHttpMiddlewareClientContext` which is generally called `context`,
 - During `Invoke` the `await _next(context);` must be invoked and return the response, in order to continue the flow.

 The following is the timer middleware implementation *(bit simplified)*.

```cs
public class TimerHttpMiddleware : IFluentHttpMiddleware
{
    private readonly FluentHttpMiddlewareDelegate _next;
    private readonly TimerHttpMiddlewareOptions _options;
    private readonly ILogger _logger;

    public TimerHttpMiddleware(
      FluentHttpMiddlewareDelegate next, // this needs to be here and should be first
      FluentHttpMiddlewareClientContext context, // this needs to be here and should be second
      TimerHttpMiddlewareOptions options,
      ILoggerFactory loggerFactory
    )
    {
        _next = next;
        _options = options;
        _logger = loggerFactory.CreateLogger($"{typeof(TimerHttpMiddleware).Namespace}.{context.Identifier}.Timer");
    }

    public async Task<FluentHttpResponse> Invoke(FluentHttpMiddlewareContext context)
    {
        var watch = Stopwatch.StartNew();
        var response = await _next(context); // this needs to be invoked to continue middleware flow
        var elapsed = watch.Elapsed;
        _logger.LogInformation("Executed request {request} in {timeTakenMillis}ms", context.Request, elapsed.TotalMilliseconds);
        response.SetTimeTaken(elapsed);
        return response;
    }
}

namespace FluentlyHttpClient
{
    // Response extension methods - useful to extend FluentHttpResponse
    public static class TimerFluentResponseExtensions
    {
        private const string TimeTakenKey = "TIMER_TIME_TAKEN";

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
Options to middleware can be passed via an argument. Note it has to be the second argument within the constructor.

```cs
public TimerHttpMiddleware(
  FluentHttpMiddlewareDelegate next,
  FluentHttpMiddlewareClientContext context,
  TimerHttpMiddlewareOptions options, // <- options should be here
  ILoggerFactory loggerFactory
)
```

Options can be passed when registering a middleware.

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

#### Extending Request Builder/Client Builder headers
In order to extend headers for both `FluentHttpClientBuilder` and `FluentHttpRequestBuilder`, the best approach would be to extend on
`IFluentHttpHeaderBuilder<T>`, this way it will be available for both. See example below.

```cs
public static class FluentHttpHeaderBuilderExtensions
{
  public static T WithBearerAuthentication<T>(this IFluentHttpHeaderBuilder<T> builder, string token)
  {
    if (string.IsNullOrEmpty(token)) throw new ArgumentNullException(nameof(token));
    builder.WithHeader(HeaderTypes.Authorization, $"{AuthSchemeTypes.Bearer} {token}");
    return (T)builder;
  }
```
#### Extending Request/Response items
In order to extend `Items` for both `FluentHttpRequest` and `FluentHttpResponse`, its best to extend `IFluentHttpMessageState`.
This way it will be available for both. See example below.

```cs
public static IDictionary<string, string> GetErrorCodeMappings(this IFluentHttpMessageState message)
{
  if (message.Items.TryGetValue(ErrorCodeMappingKey, out var value))
    return (IDictionary<string, string>)value;
  return null;
}
```

### Recipes

#### File upload

```cs
var multiForm = new MultipartFormDataContent
{
  { "hero", "Jaina" }
};
multiForm.AddFile("file", "./animal-mustache.jpg");

var response = await httpClient.CreateRequest("/api/sample/upload")
  .AsPost()
  .WithBodyContent(multiForm)
  .ReturnAsResponse<MyResult>();
```

#### Simple Single file HttpClient
Even though in general we do not suggest (unless for small HttpClients) at times its useful to create a simple quick way http client.

```cs
public class SelfInfoHttpClient
{
  private readonly IFluentHttpClient _httpClient;

  public SelfInfoHttpClient(
    IFluentHttpClientFactory httpClientFactory
  )
  {
    _httpClient = httpClientFactory.CreateBuilder("localhost")
      .WithBaseUrl($"http://localhost:5500}")
      .Build();
  }

  public Task<FluentHttpResponse> GetInfo()
    => _httpClient.CreateRequest("info")
      .AsGet()
      .ReturnAsResponse();
}
```

### Testing/Mocking
In order to test HTTP requests, the library itself doesn't offer anything out of the box.
However, we've been using [RichardSzalay.MockHttp](https://github.com/richardszalay/mockhttp), which we recommend.

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

    var httpClient = fluentHttpClientFactory.CreateBuilder("platform")
      .WithBaseUrl("https://sketch7.com")
      .AddMiddleware<TimerHttpMiddleware>()
      .WithMessageHandler(mockHttp) // set message handler to mock
      .Build();

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