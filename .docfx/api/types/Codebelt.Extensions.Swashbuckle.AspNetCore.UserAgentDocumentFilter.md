---
uid: Codebelt.Extensions.Swashbuckle.AspNetCore.UserAgentDocumentFilter
example:
- *content
---

Register `UserAgentDocumentFilter` with the Swagger generation pipeline to automatically document user-agent header requirements in the OpenAPI specification. This document filter is called during Swagger generation and modifies the specification to include the user-agent header as a required parameter on all API operations. You can register it directly as shown in the example, or use the convenient `AddUserAgent()` extension method for shorter setup code:

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
            // Register UserAgentDocumentFilter to document user-agent headers
            options.DocumentFilter<UserAgentDocumentFilter>();
            
            // Or use the convenient extension method
            // options.AddUserAgent();
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
