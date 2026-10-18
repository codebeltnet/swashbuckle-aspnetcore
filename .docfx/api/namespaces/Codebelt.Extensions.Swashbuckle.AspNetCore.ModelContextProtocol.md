---
uid: Codebelt.Extensions.Swashbuckle.AspNetCore.ModelContextProtocol
summary: *content
---
The Model Context Protocol (MCP) enables standardized machine-to-machine communication for AI tool invocation. This namespace integrates MCP into ASP.NET Core OpenAPI documentation by injecting the MCP endpoint and tools into the Swagger/OpenAPI document that Swashbuckle generates.

**Start here:** Call `AddMcpServer` on `SwaggerGenOptions` during your Swagger configuration to add MCP endpoints to generated OpenAPI documents. Use the original overload to apply one configuration to every document, or the document-name overload to give each document its own MCP version and transport settings.

**When to use:** Choose this namespace when you want to expose an MCP server as part of your API documentation alongside traditional HTTP endpoints. The filter discovers and documents MCP tools and operations in the OpenAPI document.

**Usage:** Call the `AddMcpServer` extension method in your Swagger setup. The overload that accepts a document name can be called once for each MCP document, allowing each one to select its own `Version`, endpoint pattern, tag, tool discovery, and `HttpServerSessionMode` to match `MapMcp`. The default version is 2026-07-28 modern stateless Streamable HTTP; select 2025-11-25, 2025-06-18, or 2025-03-26 for an earlier Streamable HTTP contract, or 2024-11-05 for the legacy `/sse` and `/message` transport. Stateful and hybrid modes add the session operations for Streamable HTTP, and legacy SSE remains opt-in for newer revisions.

[!INCLUDE [availability-modern](../../includes/availability-modern.md)]

Implements: [Model Context Protocol (MCP)](https://modelcontextprotocol.io) 🔗
Related: [Codebelt.Extensions.Swashbuckle.AspNetCore](../namespaces/Codebelt.Extensions.Swashbuckle.AspNetCore.md)

### Extension Members

|Type|Ext|Methods|
|--:|:-:|---|
|SwaggerGenOptions|⬇️|`AddMcpServer`|

