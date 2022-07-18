# To do
- Consider `FluentlyHttpRequestBuilder` instead `FluentlyHttpRequest` in middleware (breaking)
- Add Request Item `RawBodyData` (or so) to get the "raw" object passed in the request (which can be used in middleware for example or other extensions)
- Request url parts e.g. `CreateRequest(`[resource]/{id}`)` and `.WithUrlNamedPart("resource", "user-roles")`
- `CreateClient` `SubClientIdentityFormatter` configuration
- `DisposeOnSend` config?
- .WithItems? (from `Dictionary<string, object>`)
- `.WithQueryParams` add `append` `.WithQueryParams(new { Category = "assassin" })` chained with `.WithQueryParams(new { Page = 1 }, append: true)`
- `.WithInterpolationData` + `append` - can be used so during config pass e.g. `brand=xyz` and later used `.CreateRequest("api/{brand}")` which will be interpolated