# To do
- `CreateClient` `SubClientIdentityFormatter` configuration
- `DisposeOnSend` config?
- Consider `FluentlyHttpRequestBuilder` instead `FluentlyHttpRequest` in middleware (breaking)
- .WithItems? (from `Dictionary<string, object>`)
- `.WithQueryParams` add `append` `.WithQueryParams(new { Category = "assassin" })` chained with `.WithQueryParams(new { Page = 1 }, append: true)`
- `.WithInterpolationData` + `append` - can be used so during config pass e.g. `brand=xyz` and later used `.CreateRequest("api/{brand}")` which will be interpolated