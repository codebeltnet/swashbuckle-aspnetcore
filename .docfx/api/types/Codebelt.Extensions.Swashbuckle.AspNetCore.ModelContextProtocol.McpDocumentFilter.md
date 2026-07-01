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
            EnableLegacySse = false
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

When `McpDocumentFilter` is applied during OpenAPI document generation, it injects the MCP endpoint paths and operations. If tool discovery is enabled, the filter automatically discovers and documents available MCP tools.

