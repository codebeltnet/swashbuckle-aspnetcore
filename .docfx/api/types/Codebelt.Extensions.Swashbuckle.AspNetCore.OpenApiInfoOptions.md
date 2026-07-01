---
uid: Codebelt.Extensions.Swashbuckle.AspNetCore.OpenApiInfoOptions
example:
- *content
---

Configure OpenAPI document metadata to appear in the Swagger UI and specification. This example shows a typical setup: you call `AddRestfulSwagger` with a setup action, access the `OpenApiInfo` property to retrieve the configuration options, and then set standard OpenAPI metadata fields like title, version, description, contact information, and license. These values are merged into the generated OpenAPI specification document and displayed in the Swagger UI header and information section:

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

        // Use OpenApiInfoOptions to set document metadata
        builder.Services.AddRestfulSwagger(options =>
        {
            // Cast to OpenApiInfoOptions to demonstrate type usage
            OpenApiInfoOptions apiInfo = options.OpenApiInfo;
            apiInfo.Title = "Product API";
            apiInfo.Description = "API for managing product inventory";
            apiInfo.Contact.Name = "API Support";
            apiInfo.License.Name = "MIT";
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

