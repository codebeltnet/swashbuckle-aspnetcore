---
uid: Codebelt.Extensions.Swashbuckle.AspNetCore.ModelContextProtocol.SwaggerGenOptionsExtensions
example:
- *content
---
To enable MCP server documentation in your OpenAPI specification, call `AddMcpServer` as an extension method on a `SwaggerGenOptions` instance within your Swagger configuration. This method registers the MCP document filter and automatically injects MCP endpoint documentation into the generated OpenAPI document.

```csharp
using Codebelt.Extensions.Swashbuckle.AspNetCore.ModelContextProtocol;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace YourApp.Configuration;

/// <summary>
/// Example showing how to use the AddMcpServer extension method.
/// </summary>
public class SwaggerMcpSetup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Configure Swagger generation and add MCP server support
        services.AddSwaggerGen(options =>
        {
            // Call AddMcpServer as an extension method on the SwaggerGenOptions instance
            options.AddMcpServer();
        });
    }

    public void ConfigureServicesWithCustomOptions(IServiceCollection services)
    {
        // Or with custom MCP options
        services.AddSwaggerGen(options =>
        {
            options.AddMcpServer(o =>
            {
                o.Pattern = "/mcp";
                o.TagName = "AI Tools";
                o.IncludeTools = true;
            });
        });
    }
}
```

The `AddMcpServer` method accepts an optional configuration action for `McpDocumentOptions`. When called without arguments, it uses default values: an MCP endpoint at `/mcp`, grouped under the "MCP" tag, with automatic tool discovery enabled. The method internally creates a `McpDocumentFilter` and registers it as a document filter, so the filter is invoked during OpenAPI document generation to add MCP endpoint paths and operations.

