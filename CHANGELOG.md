# Changelog

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

For more details, please refer to `PackageReleaseNotes.txt` on a per assembly basis in the `.nuget` folder.

> [!NOTE]  
> Changelog entries prior to version 8.4.0 was migrated from previous versions of Codebelt.Extensions.Swashbuckle.AspNetCore.

## [9.0.7] - 2025-09-15

This is a service update that focuses on package dependencies.

## [9.0.6] - 2025-08-20

This is a service update that focuses on package dependencies.

## [9.0.5] - 2025-07-11

This is a service update that focuses on package dependencies.

## [9.0.4] - 2025-06-16

This is a service update that focuses on package dependencies.

## [9.0.3] - 2025-05-25

This is a service update that focuses on package dependencies.

## [9.0.2] - 2025-04-16

This is a service update that focuses on package dependencies.

## [9.0.1] - 2025-01-30

This is a service update that primarily focuses on package dependencies and minor improvements.

## [9.0.0] - 2024-11-13

This major release is first and foremost focused on ironing out any wrinkles that have been introduced with .NET 9 preview releases so the final release is production ready together with the official launch from Microsoft.

## [8.4.0] - 2024-09-22

### Dependencies

- Codebelt.Extensions.Swashbuckle.AspNetCore updated to latest and greatest with respect to TFMs

## [8.3.2] - 2024-08-04

### Dependencies

- Codebelt.Extensions.Swashbuckle.AspNetCore updated to latest and greatest with respect to TFMs


## [8.3.1] - 2024-06-01

### Dependencies

- Codebelt.Extensions.Swashbuckle.AspNetCore updated to latest and greatest with respect to TFMs


## [8.2.0] - 2024-03-03

### Added

- SwaggerGenOptionsExtensions class in the Codebelt.Extensions.Swashbuckle.AspNetCore namespace was extended with one new extension method for the SwaggerGenOptions class: AddBasicAuthenticationSecurity

### Changed

- RestfulSwaggerOptions class in the Codebelt.Extensions.Swashbuckle.AspNetCore namespace to include a function delegate property named JsonSerializerOptionsFactory that will resolve a JsonSerializerOptions instance in a more flexible way than provided by the Swagger team
- ServiceCollectionExtensions class in the Codebelt.Extensions.Swashbuckle.AspNetCore namespace to support JsonSerializerOptionsFactory in the AddRestfulSwagger extension method


## [7.0.0] 2022-11-09

### Added

- ConfigureSwaggerGenOptions class in the Codebelt.Extensions.Swashbuckle.AspNetCore namespace that represents something that configures the SwaggerGenOptions type
- ConfigureSwaggerUIOptions class in the Codebelt.Extensions.Swashbuckle.AspNetCore namespace that represents something that configures the SwaggerUIOptions type
- DocumentFilter class in the Codebelt.Extensions.Swashbuckle.AspNetCore namespace that represents the base class of an IDocumentFilter implementation
- OpenApiInfoOptions class in the Codebelt.Extensions.Swashbuckle.AspNetCore namespace that represents a proxy for configuring an Open API Info Object that provides metadata about an Open API endpoint
- OperationFilter class in the Codebelt.Extensions.Swashbuckle.AspNetCore namespace that represents the base class of an IOperationFilter implementation
- RestfulSwaggerOptions class in the Codebelt.Extensions.Swashbuckle.AspNetCore namespace that provides programmatic configuration for the ServiceCollectionExtensions.AddRestfulSwagger method
- ServiceCollectionExtensions class in the Codebelt.Extensions.Swashbuckle.AspNetCore namespace that consist of extension methods for the IServiceCollection interface: AddRestfulSwagger
- SwaggerGenOptionsExtensions class in the Codebelt.Extensions.Swashbuckle.AspNetCore namespace that consist of extension methods for the SwaggerGenOptions class: AddUserAgent, AddXApiKeySecurity, AddJwtBearerSecurity
- UserAgentDocumentFilter class in the Codebelt.Extensions.Swashbuckle.AspNetCore namespace that provides a User-Agent field to the generated OpenApiDocument
- UserAgentDocumentOptions class in the Codebelt.Extensions.Swashbuckle.AspNetCore namespace that provides programmatic configuration for the UserAgentDocumentFilter class
- XPathDocumentExtensions class in the Codebelt.Extensions.Swashbuckle.AspNetCore namespace that consist of extension methods for the XPathDocument class: AddByType, AddByAssembly, AddByFilename
