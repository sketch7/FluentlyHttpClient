# Fluently Http Client Entity

## Generate Database Scripts

### Generate Migration Script
This need to run from `FluentlyHttpClient` main directory

`Initial` in the below command is the name of the generated script.

**.NET CLI**
```bash
dotnet ef migrations add Initial --project src/FluentlyHttpClient.Entity/FluentlyHttpClient.Entity.csproj --startup-project samples/FluentlyHttpClient.Sample.Api/FluentlyHttpClient.Sample.Api.csproj
```

### Create/Update Database
There are two ways to execute the migration scripts, all migration scripts are idempotently.
 - Consumer can call `Initialize` method from `FluentHttpClientContext`.
 - Or can execute the below command in .NET CLI

**.NET CLI**
```bash
dotnet ef database update --project src/FluentlyHttpClient.Entity/FluentlyHttpClient.Entity.csproj --startup-project samples/FluentlyHttpClient.Sample.Api/FluentlyHttpClient.Sample.Api.csproj
```

## Features
- **cache:** save responses to SQL Database

## Consumers
 - Register `.AddFluentlyHttpClientEntity` in your application DI. This require a connection string for the database.
 - During bootstrap of the application call `Initialize` method from `FluentHttpClientContext`.
 - Inject `IRemoteResponseCacheService`.