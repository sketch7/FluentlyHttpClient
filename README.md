[projectUri]: https://github.com/sketch7/FluentlyHttpClient
[projectGit]: https://github.com/sketch7/FluentlyHttpClient.git
[changeLog]: ./CHANGELOG.md

# Fluently Http Client
[![CircleCI](https://circleci.com/gh/sketch7/FluentlyHttpClient.svg?style=shield)](https://circleci.com/gh/sketch7/FluentlyHttpClient)
[![NuGet version](https://badge.fury.io/nu/fluentlyhttpclient.svg)](https://badge.fury.io/nu/fluentlyhttpclient)

Fluent Http Client with a fluent APIs which are intuitive, easy to use and also highly extensible.

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
 - Url interpolation e.g. person/{id}
 - Highly extensible



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
