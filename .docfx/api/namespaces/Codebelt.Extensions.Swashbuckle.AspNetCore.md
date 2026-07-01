---
uid: Codebelt.Extensions.Swashbuckle.AspNetCore
summary: *content
---
When building RESTful APIs with ASP.NET Core, Swashbuckle.AspNetCore provides the Swagger and OpenAPI tooling. This namespace complements Swashbuckle.AspNetCore with pre-configured options, document and operation filters, and helper utilities to reduce boilerplate when documenting APIs that use standard security schemes and conventions.

**Start here:** Call `AddRestfulSwagger` on `IServiceCollection` to register both Swagger generator and UI options optimized for RESTful API patterns. This single call sets up user-agent headers, API key and bearer-token security schemes, and XML documentation inclusion with minimal configuration.

**When to use:** Choose this namespace when you are building an ASP.NET Core REST API and want to avoid repetitive Swagger configuration code. If you need granular control over individual Swagger filters or have non-standard security requirements, use Swashbuckle.AspNetCore directly and import only the extension methods you need from this namespace.

**Usage:** Configure both Swagger generation and UI in a single step with predefined filters for common security patterns, XPath document loading, and API versioning conventions.

[!INCLUDE [availability-modern](../../includes/availability-modern.md)]

Complements: [Swashbuckle.AspNetCore](https://github.com/domaindrivendev/Swashbuckle.AspNetCore) 🔗
Related: [Codebelt.Extensions.Asp.Versioning](https://versioning.codebelt.net/api/Codebelt.Extensions.Asp.Versioning.html) 📘

### Extension Members

|Type|Ext|Methods|
|--:|:-:|---|
|IServiceCollection|⬇️|`AddRestfulSwagger`|
|SwaggerGenOptions|⬇️|`AddUserAgent`, `AddXApiKeySecurity`, `AddJwtBearerSecurity`, `AddBasicAuthenticationSecurity`|
|XPathDocument|⬇️|`AddByType`, `AddByType<T>`, `AddByAssembly`, `AddByFilename`, `AddFromBaseDirectory`, `AddFromBaseDirectory<T>`, `AddFromReferencePacks`, `AddFromReferencePacks<T>`|

