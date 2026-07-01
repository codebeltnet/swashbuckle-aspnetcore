---
uid: Codebelt.Extensions.Swashbuckle.AspNetCore.ConfigureSwaggerUIOptions
example:
- *content
---

Automatically configure Swagger UI options through the ASP.NET Core Options pattern. When you call `AddRestfulSwagger`, a `ConfigureSwaggerUIOptions` instance is automatically registered as an `IConfigureOptions<SwaggerUIOptions>` implementation and applied by the framework. This example shows how the type is used internally: it retrieves the registered `ConfigureSwaggerUIOptions` from the service provider and demonstrates that it implements the configuration interface:

```csharp
using System;
using Codebelt.Extensions.Swashbuckle.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace MySwaggerExample;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        // Register RestfulSwagger which internally uses ConfigureSwaggerUIOptions
        builder.Services.AddRestfulSwagger();

        var app = builder.Build();

        if (string.Equals(app.Environment.EnvironmentName, "Development", StringComparison.OrdinalIgnoreCase))
        {
            app.UseSwagger();
            app.UseSwaggerUI();

            // ConfigureSwaggerUIOptions is automatically registered and applied by the framework
            // Retrieve the configuration service to verify ConfigureSwaggerUIOptions is present
            var configureService = app.Services.GetService(typeof(IConfigureOptions<SwaggerUIOptions>));
            if (configureService is ConfigureSwaggerUIOptions configureUI)
            {
                // ConfigureSwaggerUIOptions has been registered and will be invoked by the Options system
            }
        }

        app.MapControllers();
        app.Run();
    }
}
```

