---
uid: Codebelt.Extensions.Swashbuckle.AspNetCore.ModelContextProtocol
summary: *content
---
The Model Context Protocol (MCP) enables standardized machine-to-machine communication for AI tool invocation. This namespace integrates MCP into ASP.NET Core OpenAPI documentation by injecting the MCP endpoint and tools into the Swagger/OpenAPI document that Swashbuckle generates.

**Start here:** Call `AddMcpServer` on `SwaggerGenOptions` during your Swagger configuration to register the MCP document filter and automatically inject the MCP server endpoint into the OpenAPI specification. This single extension method handles filter registration and options configuration.

**When to use:** Choose this namespace when you want to expose an MCP server as part of your API documentation alongside traditional HTTP endpoints. The filter discovers and documents MCP tools and operations in the OpenAPI document.

**Usage:** Call the `AddMcpServer` extension method in your Swagger setup. Optionally configure the MCP endpoint pattern, tag name, tool discovery, and transport mode by passing a configuration action.

[!INCLUDE [availability-modern](../../includes/availability-modern.md)]

Implements: [Model Context Protocol (MCP)](https://modelcontextprotocol.io) 🔗
Related: [Codebelt.Extensions.Swashbuckle.AspNetCore](../namespaces/Codebelt.Extensions.Swashbuckle.AspNetCore.md)

### Extension Members

|Type|Ext|Methods|
|--:|:-:|---|
|SwaggerGenOptions|⬇️|`AddMcpServer`|

