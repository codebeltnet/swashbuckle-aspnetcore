---
uid: Codebelt.Extensions.Swashbuckle.AspNetCore.ModelContextProtocol.SwaggerGenOptionsExtensions
example:
- *content
---
To enable MCP server documentation in your OpenAPI specification, call `AddMcpServer` as an extension method on a `SwaggerGenOptions` instance within your Swagger configuration. This method registers the MCP document filter and automatically injects MCP endpoint documentation into the generated OpenAPI document.

```csharp
using Codebelt.Extensions.Swashbuckle.AspNetCore.ModelContextProtocol;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using ModelContextProtocol.AspNetCore;
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
                o.Version = "2025-11-25";
            });
        });
    }

    public void ConfigureServicesWithSeparateMcpDocuments(IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("legacy-mcp", new OpenApiInfo { Title = "Legacy MCP", Version = "2025-11-25" });
            options.SwaggerDoc("modern-mcp", new OpenApiInfo { Title = "Modern MCP", Version = "2026-07-28" });
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Initialize MCP", Version = "2026-07-28" });

            options.AddMcpServer("legacy-mcp", mcp =>
            {
                mcp.Version = "2025-11-25";
                mcp.SessionMode = HttpServerSessionMode.Stateful;
            });
            options.AddMcpServer("modern-mcp", mcp => mcp.Version = "2026-07-28");
            options.AddMcpServer("v1", mcp => mcp.SessionMode = HttpServerSessionMode.StatefulForInitializeClients);
        });
    }
}
```

The overload without a document name accepts an optional configuration action for `McpDocumentOptions`. When called without arguments, it uses the defaults: an MCP endpoint at `/mcp`, grouped under the "MCP" tag, with automatic tool discovery enabled. The document-name overload accepts a required configuration action and applies those options only when Swashbuckle generates the matching document. Call it once per document to publish different MCP specification revisions or session modes from the same application.

