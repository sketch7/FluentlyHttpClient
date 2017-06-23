# Fluently Http Client
Fluent Http Client with a fluent APIs which are intuitive and easy to use. It's also highly extensible.

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


## Getting Started

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
