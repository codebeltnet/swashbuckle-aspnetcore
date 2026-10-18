---
uid: Codebelt.Extensions.Swashbuckle.AspNetCore.ModelContextProtocol.McpDocumentOptions
example:
- *content
---
Use `McpDocumentOptions` to customize how the MCP filter documents your server in the OpenAPI specification. Instantiate and configure options, then pass them to the filter or use with the convenience extension method.

```csharp
using Codebelt.Extensions.Swashbuckle.AspNetCore.ModelContextProtocol;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.AspNetCore;
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
                SessionMode = HttpServerSessionMode.Stateful,
                EnableLegacySse = true,
                Version = "2025-11-25"
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
- **Version:** Selects the MCP specification revision documented by the filter (default: `2026-07-28`). Supported values are `2026-07-28`, `2025-11-25`, `2025-06-18`, `2025-03-26`, and `2024-11-05`.
- **SessionMode:** Must match the `HttpServerTransportOptions.SessionMode` used by `MapMcp`; defaults to `HttpServerSessionMode.Stateless`. Stateless mode documents only `POST`, while stateful and hybrid modes also document the session `GET` and `DELETE` operations.
- **EnableLegacySse:** Includes legacy HTTP+SSE transport endpoints alongside a Streamable HTTP revision (default: `false`). The 2024-11-05 selection documents the legacy `/sse` and `/message` endpoints as its primary transport.

Call `ValidateOptions` to verify the configured options. It throws `InvalidOperationException` when `Version` is blank or is not one of the five supported MCP specification revisions.

