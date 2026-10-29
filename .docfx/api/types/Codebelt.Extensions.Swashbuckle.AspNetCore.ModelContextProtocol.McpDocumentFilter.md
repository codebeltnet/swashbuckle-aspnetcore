---
uid: Codebelt.Extensions.Swashbuckle.AspNetCore.ModelContextProtocol.McpDocumentFilter
example:
- *content
---
To add MCP server documentation to your OpenAPI specification, you can either use the `AddMcpServer` extension method (recommended for standard setup), or directly instantiate and configure `McpDocumentFilter` with custom options when you need fine-grained control over the filter.

```csharp
using Codebelt.Extensions.Swashbuckle.AspNetCore.ModelContextProtocol;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace YourApp.Configuration;

/// <summary>
/// Example showing how to use McpDocumentFilter directly.
/// </summary>
public class McpDocumentFilterExample
{
    public void ConfigureMcp()
    {
        // Create custom MCP options
        var mcpOptions = new McpDocumentOptions
        {
            Pattern = "/api/mcp",
            TagName = "AI Server",
            IncludeTools = true,
            SessionMode = ModelContextProtocol.AspNetCore.HttpServerSessionMode.Stateless,
            EnableLegacySse = false,
            Version = "2025-11-25"
        };

        // Instantiate the document filter directly
        var mcpFilter = new McpDocumentFilter(mcpOptions);

        // Register the filter in Swagger configuration
        var swaggerOptions = new SwaggerGenOptions();
        swaggerOptions.DocumentFilterDescriptors.Add(
            new FilterDescriptor
            {
                Type = typeof(McpDocumentFilter),
                Arguments = new object[] { mcpOptions }
            }
        );
    }
}
```

When `McpDocumentFilter` is applied during OpenAPI document generation, it uses `McpDocumentOptions.Version` to select the MCP contract. The default is 2026-07-28, which documents modern Streamable HTTP with per-request metadata and protocol headers. The 2025 revisions document the earlier initialize-handshake Streamable HTTP contract, and 2024-11-05 documents the separate legacy HTTP+SSE endpoints. For Streamable HTTP, the documented root operations follow `SessionMode`: stateless mode exposes only `POST`, while stateful and hybrid modes also expose the session `GET` and `DELETE` operations. If tool discovery is enabled, the filter automatically discovers and documents available MCP tools.

