---
uid: Codebelt.Extensions.Swashbuckle.AspNetCore.RestfulSwaggerOptions
example:
- *content
---

Customize Swagger generation and UI settings for a RESTful API. The `RestfulSwaggerOptions` type bundles all configuration aspects: OpenAPI document metadata, XML documentation loading, and schema generation options. This example shows how to configure core settings:

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

        // Configure RestfulSwaggerOptions with customized settings
        builder.Services.AddRestfulSwagger(rawOptions =>
        {
            // Explicitly declare RestfulSwaggerOptions type
            RestfulSwaggerOptions options = rawOptions;

            // Configure document metadata through OpenApiInfo property
            options.OpenApiInfo.Title = "My REST API";
            options.OpenApiInfo.Description = "A sample API documentation";
            options.OpenApiInfo.Contact.Name = "API Support";

            // Enable XML documentation comments
            options.XmlDocumentations.AddByType<Program>();

            // Configure Swagger generation through Settings
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

