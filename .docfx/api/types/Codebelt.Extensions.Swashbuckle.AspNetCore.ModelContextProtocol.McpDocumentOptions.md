---
uid: Codebelt.Extensions.Swashbuckle.AspNetCore.ModelContextProtocol.McpDocumentOptions
example:
- *content
---
Use `McpDocumentOptions` to customize how the MCP filter documents your server in the OpenAPI specification. Instantiate and configure options, then pass them to the filter or use with the convenience extension method.

```csharp
using Codebelt.Extensions.Swashbuckle.AspNetCore.ModelContextProtocol;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace YourApp.Configuration;

/// <summary>
/// Example showing how to configure McpDocumentOptions.
/// </summary>
public class McpDocumentOptionsExample
{
    public void ConfigureSwagger(IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            // Create and customize McpDocumentOptions directly
            var mcpOptions = new McpDocumentOptions
            {
                Pattern = "/api/mcp",
                TagName = "Machine Intelligence",
                IncludeTools = true,
                EnableLegacySse = true
            };

            // Register the filter with custom options
            options.DocumentFilterDescriptors.Add(
                new FilterDescriptor
                {
                    Type = typeof(McpDocumentFilter),
                    Arguments = new object[] { mcpOptions }
                }
            );
        });
    }
}
```

Each property of `McpDocumentOptions` controls how MCP endpoints appear in the OpenAPI document:
- **Pattern:** The HTTP route for MCP requests (default: `/mcp`).
- **TagName:** The OpenAPI tag grouping MCP operations (default: `MCP`).
- **IncludeTools:** Enables automatic tool discovery and documentation (default: `true`).
- **EnableLegacySse:** Includes legacy HTTP+SSE transport endpoints (default: `false`).

