# To do
- `CreateClient` `SubClientIdentityFormatter` configuration
- File Upload
  - Via Path e.g. read file + take data from it
  - Via `WithFileBody()`
    `.FromKind(FileKind.Image)` -- this will set type header
  - **NOTES:**
    - HttpClient https://stackoverflow.com/questions/16416601/c-sharp-httpclient-4-5-multipart-form-data-upload
    - ASPNET https://docs.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads?view=aspnetcore-2.1