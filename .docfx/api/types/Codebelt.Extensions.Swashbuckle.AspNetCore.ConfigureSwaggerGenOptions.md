---
uid: Codebelt.Extensions.Swashbuckle.AspNetCore.ConfigureSwaggerGenOptions
example:
- *content
---

Automatically configure Swagger generation options through the ASP.NET Core Options pattern. When you call `AddRestfulSwagger`, a `ConfigureSwaggerGenOptions` instance is automatically registered as an `IConfigureOptions<SwaggerGenOptions>` implementation and applied by the framework. This example shows how the type is used internally: it retrieves the registered `ConfigureSwaggerGenOptions` from the service provider and demonstrates that it implements the configuration interface:

```csharp
using System;
using Codebelt.Extensions.Swashbuckle.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MySwaggerExample;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        // Register RestfulSwagger which internally uses ConfigureSwaggerGenOptions
        builder.Services.AddRestfulSwagger(options =>
        {
            options.OpenApiInfo.Title = "My API";
        });

        var app = builder.Build();

        // ConfigureSwaggerGenOptions is automatically registered and applied by the framework
        // Retrieve the configuration service to verify ConfigureSwaggerGenOptions is present
        var configureService = app.Services.GetService(typeof(IConfigureOptions<SwaggerGenOptions>));
        if (configureService is ConfigureSwaggerGenOptions configureSwagger)
        {
            // ConfigureSwaggerGenOptions has been registered and will be invoked by the Options system
        }

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

