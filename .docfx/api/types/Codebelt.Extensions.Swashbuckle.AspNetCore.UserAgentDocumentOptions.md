---
uid: Codebelt.Extensions.Swashbuckle.AspNetCore.UserAgentDocumentOptions
example:
- *content
---

Control the documentation of user-agent header in the OpenAPI specification. This example demonstrates the full setup workflow: you call `AddSwaggerGen` with a setup action, then call `AddUserAgent` with a configuration action, and within that action you configure the `UserAgentDocumentOptions` to control whether the header is required and what description appears in the API documentation. The configured options are then applied globally to all endpoints in the generated OpenAPI specification:

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

        builder.Services.AddSwaggerGen(options =>
        {
            // Configure user-agent header documentation using UserAgentDocumentOptions
            options.AddUserAgent(userAgentOptionsParam =>
            {
                // Cast to UserAgentDocumentOptions to demonstrate type usage
                UserAgentDocumentOptions userAgentOptions = userAgentOptionsParam;
                
                userAgentOptions.Required = true;
                userAgentOptions.Description = "Client application identifier";
            });
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
