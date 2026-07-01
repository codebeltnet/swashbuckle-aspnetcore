---
uid: Codebelt.Extensions.Swashbuckle.AspNetCore.ServiceCollectionExtensions
example:
- *content
---

Register comprehensive Swagger documentation for a RESTful API. This extension method is the entry point to add OpenAPI services to the dependency injection container. Call it during application startup in `Program.cs` to enable both Swagger generation and Swagger UI. The example shows the basic setup with metadata configuration and XML documentation loading:

```csharp
using System;
using Codebelt.Extensions.Swashbuckle.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace MySwaggerExample;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        // Use ServiceCollectionExtensions.AddRestfulSwagger to configure Swagger
        builder.Services.AddRestfulSwagger(options =>
        {
            options.OpenApiInfo.Title = "My API";
            options.OpenApiInfo.Description = "API with comprehensive documentation";
            options.XmlDocumentations.AddByType<Program>();
        });

        var app = builder.Build();

        if (string.Equals(app.Environment.EnvironmentName, "Development", StringComparison.OrdinalIgnoreCase))
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.MapControllers();
        app.Run();
    }
}
```

---
uid: Codebelt.Extensions.Swashbuckle.AspNetCore.ServiceCollectionExtensions.AddRestfulSwagger(Microsoft.Extensions.DependencyInjection.IServiceCollection)
example:
- *content
---

Configure both Swagger generation and UI options for a RESTful API using default settings. Call this parameterless overload when no custom configuration is needed, and the extension method will automatically set up both SwaggerGen and SwaggerUI with sensible RESTful API defaults. The example demonstrates a minimal setup that enables all-of schema extensions:

```csharp
using System;
using Codebelt.Extensions.Swashbuckle.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace MySwaggerExample;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddRestfulSwagger(options =>
        {
            options.Settings.UseAllOfToExtendReferenceSchemas();
            options.XmlDocumentations.AddByType<Program>();
        });

        var app = builder.Build();

        if (string.Equals(app.Environment.EnvironmentName, "Development", StringComparison.OrdinalIgnoreCase))
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}
```

---
uid: Codebelt.Extensions.Swashbuckle.AspNetCore.ServiceCollectionExtensions.AddRestfulSwagger(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Action{Codebelt.Extensions.Swashbuckle.AspNetCore.RestfulSwaggerOptions})
example:
- *content
---

Configure Swagger with custom settings through the `RestfulSwaggerOptions` parameter. This overload accepts a setup action where you can customize API metadata, enable XML documentation loading, and configure advanced SwaggerGen behavior. The example shows how to set document title and version, enable controller XML comments, add API-wide documentation, and use the all-of schema extension for better reference-type composition:

```csharp
using System;
using Codebelt.Extensions.Swashbuckle.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace MySwaggerExample;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddRestfulSwagger(options =>
        {
            options.OpenApiInfo.Title = "Custom API";
            options.OpenApiInfo.Description = "Customized API documentation";
            options.IncludeControllerXmlComments = true;
            options.XmlDocumentations.AddByType<Program>();
            options.Settings.UseAllOfToExtendReferenceSchemas();
        });

        var app = builder.Build();

        if (string.Equals(app.Environment.EnvironmentName, "Development", StringComparison.OrdinalIgnoreCase))
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.MapControllers();
        app.Run();
    }
}
```
