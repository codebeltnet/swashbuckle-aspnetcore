# Changelog

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

For more details, please refer to `PackageReleaseNotes.txt` on a per assembly basis in the `.nuget` folder.

> [!NOTE]  
> Changelog entries prior to version 8.4.0 was migrated from previous versions of Codebelt.Extensions.Swashbuckle.AspNetCore.

## [10.1.1] - 2026-03-29

This is a patch release focused on dependency upgrades across all supported target frameworks, a build-system simplification for package release notes, and minor CI pipeline and tooling improvements.

### Changed

- `Swashbuckle.AspNetCore` dependency upgraded from 10.1.4 to 10.1.7,
- `Microsoft.AspNetCore.OpenApi` dependency upgraded from 10.0.3 to 10.0.5,
- `Codebelt.Bootstrapper.Web` dependency upgraded from 5.0.4 to 5.0.5,
- `Codebelt.Extensions.Asp.Versioning` dependency upgraded from 10.0.4 to 10.0.5,
- `Codebelt.Extensions.Xunit.App` dependency upgraded from 11.0.7 to 11.0.8,
- `Cuemon.Extensions.AspNetCore.Mvc.Formatters.Text.Json` dependency upgraded from 10.4.0 to 10.5.0,
- `coverlet.collector` and `coverlet.msbuild` dependencies upgraded from 8.0.0 to 8.0.1,
- `Directory.Build.targets` simplified `PackageReleaseNotes` injection to use `File.ReadAllText` instead of line-by-line reading,
- test environments updated to use versioned Docker image tags (`ubuntu-testrunner:9` and `ubuntu-testrunner:10`) with explicit net9/net10 environment names,
- service-update workflow corrected blank-line formatting in the release-notes entry template,
- `bump-nuget.py` script extended with the `carter` repository-to-package mapping.

## [10.1.0] - 2026-03-01

This is a minor release that focuses on XML documentation loading reliability and package dependencies.

### Added

- `XPathDocumentExtensions` class in the Codebelt.Extensions.Swashbuckle.AspNetCore namespace with several new extension methods for the `IList<XPathDocument>` class: `AddByType<T>`, `AddFromBaseDirectory`, `AddFromBaseDirectory<T>`, `AddFromReferencePacks` and `AddFromReferencePacks<T>`

## [10.0.3] - 2026-02-21

This is a service update that focuses on package dependencies.

### Fixed

- `AddRestfulSwagger` extension method on the `ServiceCollectionExtensions` class in the Codebelt.Extensions.Swashbuckle.AspNetCore namespace was fixed to only be invoked once on bootstrapping


## [10.0.2] - 2026-02-15

This is a service update that focuses on package dependencies.

## [10.0.1] - 2026-01-23

This is a service update that focuses on package dependencies.

## [10.0.0] - 2025-11-13

This is a major release that focuses on adapting the latest `.NET 10` release (LTS) in exchange for current `.NET 8` (LTS).

> To ensure access to current features, improvements, and security updates, and to keep the codebase clean and easy to maintain, we target only the latest long-term (LTS), short-term (STS) and (where applicable) cross-platform .NET versions.

## [9.0.8] - 2025-10-20

This is a service update that focuses on package dependencies.

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

[Unreleased]: https://github.com/codebeltnet/swashbuckle-aspnetcore/compare/v10.1.1...HEAD
[10.1.1]: https://github.com/codebeltnet/swashbuckle-aspnetcore/compare/v10.1.0...v10.1.1
[10.1.0]: https://github.com/codebeltnet/swashbuckle-aspnetcore/compare/v10.0.3...v10.1.0
[10.0.3]: https://github.com/codebeltnet/swashbuckle-aspnetcore/compare/v10.0.2...v10.0.3
[10.0.2]: https://github.com/codebeltnet/swashbuckle-aspnetcore/compare/v10.0.1...v10.0.2
[10.0.1]: https://github.com/codebeltnet/swashbuckle-aspnetcore/compare/v10.0.0...v10.0.1
[10.0.0]: https://github.com/codebeltnet/swashbuckle-aspnetcore/compare/v9.0.8...v10.0.0
[9.0.8]: https://github.com/codebeltnet/swashbuckle-aspnetcore/compare/v9.0.7...v9.0.8
[9.0.7]: https://github.com/codebeltnet/swashbuckle-aspnetcore/compare/v9.0.6...v9.0.7
[9.0.6]: https://github.com/codebeltnet/swashbuckle-aspnetcore/compare/v9.0.5...v9.0.6
[9.0.5]: https://github.com/codebeltnet/swashbuckle-aspnetcore/compare/v9.0.4...v9.0.5
[9.0.4]: https://github.com/codebeltnet/swashbuckle-aspnetcore/compare/v9.0.3...v9.0.4
[9.0.3]: https://github.com/codebeltnet/swashbuckle-aspnetcore/compare/v9.0.2...v9.0.3
[9.0.2]: https://github.com/codebeltnet/swashbuckle-aspnetcore/compare/v9.0.1...v9.0.2
[9.0.1]: https://github.com/codebeltnet/swashbuckle-aspnetcore/compare/v9.0.0...v9.0.1
[9.0.0]: https://github.com/codebeltnet/swashbuckle-aspnetcore/compare/v8.4.0...v9.0.0
[8.4.0]: https://github.com/codebeltnet/swashbuckle-aspnetcore/compare/v8.3.2...v8.4.0
[8.3.2]: https://github.com/codebeltnet/swashbuckle-aspnetcore/compare/v8.3.1...v8.3.2
[8.3.1]: https://github.com/codebeltnet/swashbuckle-aspnetcore/compare/v8.2.0...v8.3.1
[8.2.0]: https://github.com/codebeltnet/swashbuckle-aspnetcore/compare/v7.0.0...v8.2.0
[7.0.0]: https://github.com/codebeltnet/swashbuckle-aspnetcore/releases/tag/v7.0.0
