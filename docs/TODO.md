# To do
- Interface across `IFluentHttpHeaderBuilder` and `FluentHttpHeaders`, so extension methods can be applied to both
- Wrapper `Send` for raw `HttpRequestMessage`, so just in case unsupported features can be send with that instead
  - e.g. Task<FluentHttpResponse> Send(HttpRequestMessage request)
- `CreateClient` `SubClientIdentityFormatter` configuration
- File Upload
  - Via Path e.g. read file + take data from it
  - Via `WithFileBody()`
    `.FromKind(FileKind.Image)` -- this will set type header
  - **NOTES:**
    - HttpClient https://stackoverflow.com/questions/16416601/c-sharp-httpclient-4-5-multipart-form-data-upload
    - ASPNET https://docs.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads?view=aspnetcore-2.1