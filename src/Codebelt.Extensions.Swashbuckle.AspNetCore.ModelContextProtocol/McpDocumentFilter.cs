using Cuemon.Reflection;
using Microsoft.OpenApi;
using ModelContextProtocol.AspNetCore;
using ModelContextProtocol.Server;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading;

namespace Codebelt.Extensions.Swashbuckle.AspNetCore.ModelContextProtocol;

/// <summary>
/// A Swashbuckle <see cref="IDocumentFilter"/> that injects the modern MCP Streamable HTTP transport
/// endpoint (and optionally the legacy SSE endpoints) into the generated OpenAPI document.
/// </summary>
/// <seealso cref="DocumentFilter{T}"/>
/// <seealso cref="McpDocumentOptions"/>
public class McpDocumentFilter : DocumentFilter<McpDocumentOptions>
{
    private const string ModernProtocolVersion = "2026-07-28";

    /// <summary>
    /// Initializes a new instance of the <see cref="McpDocumentFilter"/> class.
    /// </summary>
    /// <param name="options">The options that control which document and routes are documented.</param>
    public McpDocumentFilter(McpDocumentOptions options) : base(options)
    {
    }

    /// <summary>
    /// Applies post-processing to the <paramref name="swaggerDoc"/>.
    /// </summary>
    /// <param name="swaggerDoc">The <see cref="OpenApiDocument"/> to modify.</param>
    /// <param name="context">The <see cref="DocumentFilterContext"/> that provides additional context.</param>
    /// <remarks>Injects the MCP Streamable HTTP transport endpoint (and optionally the legacy SSE endpoints) into the generated OpenAPI document.</remarks>
    public override void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        if (Options.EnableLegacySse && Options.SessionMode == HttpServerSessionMode.Stateless)
        {
            throw new InvalidOperationException("Legacy SSE requires a stateful MCP HTTP session mode.");
        }

        swaggerDoc.Tags ??= new HashSet<OpenApiTag>();
        if (swaggerDoc.Tags.All(t => t.Name != Options.TagName))
        {
            swaggerDoc.Tags.Add(new OpenApiTag
            {
                Name = Options.TagName,
                Description = "Model Context Protocol (MCP) — machine-to-machine interface for AI tool invocation."
            });
        }

        var tools = Options.IncludeTools ? McpDocumentFilter.DiscoverTools() : null;

        swaggerDoc.Paths ??= new OpenApiPaths();
        var streamableHttpPathItem = CreateStreamableHttpPathItem(swaggerDoc, tools);
        if (Options.SessionMode != HttpServerSessionMode.Stateless)
        {
            streamableHttpPathItem.Operations[HttpMethod.Get] = CreateSessionGetOperation(swaggerDoc);
            streamableHttpPathItem.Operations[HttpMethod.Delete] = CreateSessionDeleteOperation(swaggerDoc);
        }

        swaggerDoc.Paths.Add(Options.Pattern, streamableHttpPathItem);

        if (Options.EnableLegacySse)
        {
            swaggerDoc.Paths.Add(Options.Pattern + "/sse", CreateSsePathItem(swaggerDoc));
            swaggerDoc.Paths.Add(Options.Pattern + "/message", CreateSseMessagePathItem(swaggerDoc));
        }
    }

    private static List<McpToolInfo> DiscoverTools()
    {
        var types = AssemblyContext.GetCurrentDomainAssemblies(o =>
                o.AssemblyFilter = a => !a.IsDynamic)
            .SelectMany(GetAssemblyTypes)
            .Where(t => t.GetCustomAttribute<McpServerToolTypeAttribute>() != null);

        var tools = new List<McpToolInfo>();
        foreach (var type in types)
        {
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => m.GetCustomAttribute<McpServerToolAttribute>() != null))
            {
                var toolAttr = method.GetCustomAttribute<McpServerToolAttribute>()!;
                var toolName = toolAttr.Name ?? ToSnakeCaseName(method.Name);
                var toolDescription = method.GetCustomAttribute<DescriptionAttribute>()?.Description;
                var parameters = method.GetParameters()
                    .Where(p => p.ParameterType != typeof(CancellationToken) &&
                               p.ParameterType.FullName?.StartsWith("ModelContextProtocol", StringComparison.Ordinal) != true)
                    .Select(p => (ParamName: p.Name!, ParamType: p.ParameterType))
                    .ToList();
                tools.Add(new McpToolInfo(toolName, toolDescription, parameters));
            }
        }

        return tools;
    }

    private static IEnumerable<Type> GetAssemblyTypes(Assembly assembly)
    {
        try { return assembly.GetTypes(); }
        catch (ReflectionTypeLoadException ex) { return ex.Types.Where(t => t != null).Cast<Type>(); }
        catch { return Array.Empty<Type>(); }
    }

    private static JsonObject BuildExampleBody(McpToolInfo tool)
    {
        var paramsObject = new JsonObject { ["name"] = JsonValue.Create(tool.Name) };
        if (tool.Parameters.Count > 0)
        {
            var arguments = new JsonObject();
            foreach (var (paramName, paramType) in tool.Parameters)
                arguments[paramName] = GetDefaultExampleValue(paramType);
            paramsObject["arguments"] = arguments;
        }

        paramsObject["_meta"] = new JsonObject
        {
            ["io.modelcontextprotocol/protocolVersion"] = JsonValue.Create(McpDocumentFilter.ModernProtocolVersion),
            ["io.modelcontextprotocol/clientCapabilities"] = new JsonObject()
        };

        return new JsonObject
        {
            ["jsonrpc"] = JsonValue.Create("2.0"),
            ["id"] = JsonValue.Create(1),
            ["method"] = JsonValue.Create("tools/call"),
            ["params"] = paramsObject
        };
    }

    private static JsonNode GetDefaultExampleValue(Type type)
    {
        if (type == typeof(string)) return JsonValue.Create("")!;
        if (type == typeof(bool)) return JsonValue.Create(false)!;
        if (type == typeof(int) || type == typeof(long) || type == typeof(short)) return JsonValue.Create(0)!;
        if (type == typeof(double) || type == typeof(float) || type == typeof(decimal)) return JsonValue.Create(0.0)!;
        return new JsonObject();
    }

    private static string ToSnakeCaseName(string name) =>
        Regex.Replace(name, "([a-z0-9])([A-Z])", "$1_$2").ToLowerInvariant();

    private static string BuildStreamableHttpDescription(IReadOnlyList<McpToolInfo> tools)
    {
        var description = """
            Bidirectional JSON-RPC 2.0 endpoint following the
            [MCP Streamable HTTP specification (2026-07-28)](https://modelcontextprotocol.io/specification/2026-07-28/basic/transports/streamable-http).

            **Request** — send one JSON-RPC 2.0 request or notification object as an HTTP `POST`. Modern requests
            carry `io.modelcontextprotocol/protocolVersion` and `io.modelcontextprotocol/clientCapabilities` in
            `params._meta` and mirror the protocol version and method in the required HTTP headers. The modern
            `server/discover` request is sent to this same endpoint.

            **Response** — the server returns either:
            - `application/json` for a complete single response, or
            - `text/event-stream` when the server opens a streaming response with one or more SSE events.

            A `202 Accepted` with an empty body is returned for accepted notifications. Stateless mode is the modern
            default and exposes only this `POST` operation; stateful and hybrid modes additionally expose `GET` and
            `DELETE` for initialize-handshake clients. Legacy SSE is a separate, opt-in compatibility transport.
            """;

        if (tools?.Count > 0)
        {
            var rows = string.Join("\n", tools.Select(t =>
            {
                var paramsCol = t.Parameters.Count > 0
                    ? string.Join(", ", t.Parameters.Select(p => $"`{p.ParamName}` ({GetCSharpTypeName(p.ParamType)})"))
                    : "—";
                return $"| `{t.Name}` | {paramsCol} | {(t.Description ?? t.Name).Replace("|", "\\|")} |";
            }));

            description += "\n\n---\n\n**Available tools** — use the *Examples* dropdown to pre-fill the request body.\n\n| Tool | Parameters | Description |\n|---|---|---|\n" + rows;
        }

        return description;
    }

    private static string GetCSharpTypeName(Type type)
    {
        if (type == typeof(string)) return "string";
        if (type == typeof(int) || type == typeof(long) || type == typeof(short)) return "integer";
        if (type == typeof(bool)) return "boolean";
        if (type == typeof(double) || type == typeof(float) || type == typeof(decimal)) return "number";
        return type.Name;
    }

    private sealed record McpToolInfo(string Name, string Description, IReadOnlyList<(string ParamName, Type ParamType)> Parameters);

    private OpenApiPathItem CreateStreamableHttpPathItem(OpenApiDocument swaggerDoc, IReadOnlyList<McpToolInfo> tools)
    {
        var requestMediaType = new OpenApiMediaType { Schema = CreateJsonRpc2RequestSchema() };
        if (tools?.Count > 0)
            requestMediaType.Examples = tools.ToDictionary(
                t => t.Name,
                t => (IOpenApiExample)new OpenApiExample { Summary = t.Name, Value = McpDocumentFilter.BuildExampleBody(t) }
            );

        return new OpenApiPathItem
        {
            Operations = new Dictionary<HttpMethod, OpenApiOperation>
            {
                [HttpMethod.Post] = new OpenApiOperation
                {
                    Tags = new HashSet<OpenApiTagReference> { new(Options.TagName, swaggerDoc, null) },
                    Summary = "Streamable HTTP",
                    Description = McpDocumentFilter.BuildStreamableHttpDescription(tools),
                    OperationId = "mcp",
                    Parameters = CreateModernHttpParameters(),
                    RequestBody = new OpenApiRequestBody
                    {
                        Required = true,
                        Description = "A single JSON-RPC 2.0 request or notification object. Modern Streamable HTTP does not accept batch arrays.",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            ["application/json"] = requestMediaType
                        }
                    },
                    Responses = new OpenApiResponses
                    {
                        ["200"] = new OpenApiResponse
                        {
                            Description = "JSON-RPC 2.0 response (single) or SSE stream (multi-event).",
                            Content = new Dictionary<string, OpenApiMediaType>
                            {
                                ["application/json"] = new OpenApiMediaType
                                {
                                    Schema = CreateJsonRpc2ResponseSchema()
                                },
                                ["text/event-stream"] = new OpenApiMediaType
                                {
                                    Schema = new OpenApiSchema { Type = JsonSchemaType.String, Description = "Server-Sent Events stream of JSON-RPC 2.0 response objects." }
                                }
                            }
                        },
                        ["202"] = new OpenApiResponse
                        {
                            Description = "Accepted — no response body (notifications or fire-and-forget requests)."
                        }
                    }
                }
            }
        };
    }

    private static List<IOpenApiParameter> CreateModernHttpParameters() =>
    [
        new OpenApiParameter
        {
            Name = "Accept",
            In = ParameterLocation.Header,
            Required = true,
            Description = "Must list both application/json and text/event-stream.",
            Schema = new OpenApiSchema { Type = JsonSchemaType.String }
        },
        new OpenApiParameter
        {
            Name = "MCP-Protocol-Version",
            In = ParameterLocation.Header,
            Required = true,
            Description = "Modern protocol version. Must match params._meta.io.modelcontextprotocol/protocolVersion.",
            Schema = new OpenApiSchema
            {
                Type = JsonSchemaType.String,
                Enum = [JsonValue.Create(McpDocumentFilter.ModernProtocolVersion)]
            }
        },
        new OpenApiParameter
        {
            Name = "Mcp-Method",
            In = ParameterLocation.Header,
            Required = true,
            Description = "Must match the JSON-RPC method.",
            Schema = new OpenApiSchema { Type = JsonSchemaType.String }
        },
        new OpenApiParameter
        {
            Name = "Mcp-Name",
            In = ParameterLocation.Header,
            Description = "Required for tools/call, resources/read, and prompts/get; must match the name or URI in params.",
            Schema = new OpenApiSchema { Type = JsonSchemaType.String }
        }
    ];

    private OpenApiOperation CreateSessionGetOperation(OpenApiDocument swaggerDoc) => new()
    {
        Tags = new HashSet<OpenApiTagReference> { new(Options.TagName, swaggerDoc, null) },
        Summary = "Streamable HTTP — session event stream",
        Description = "Opens the optional session event stream for stateful initialize-handshake clients. It is not mapped in stateless mode and returns `405 Method Not Allowed` for modern 2026-07-28 requests.",
        OperationId = "mcp-session-stream",
        Responses = new OpenApiResponses
        {
            ["200"] = new OpenApiResponse
            {
                Description = "SSE stream for session-scoped messages.",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["text/event-stream"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema { Type = JsonSchemaType.String, Description = "Server-Sent Events stream." }
                    }
                }
            },
            ["405"] = new OpenApiResponse { Description = "Method Not Allowed for stateless or modern protocol requests." }
        }
    };

    private OpenApiOperation CreateSessionDeleteOperation(OpenApiDocument swaggerDoc) => new()
    {
        Tags = new HashSet<OpenApiTagReference> { new(Options.TagName, swaggerDoc, null) },
        Summary = "Streamable HTTP — terminate session",
        Description = "Terminates a stateful initialize-handshake session. It is not mapped in stateless mode and returns `405 Method Not Allowed` for modern 2026-07-28 requests.",
        OperationId = "mcp-session-delete",
        Responses = new OpenApiResponses
        {
            ["200"] = new OpenApiResponse { Description = "Session terminated." },
            ["404"] = new OpenApiResponse { Description = "Session not found." },
            ["405"] = new OpenApiResponse { Description = "Method Not Allowed for stateless or modern protocol requests." }
        }
    };

    private OpenApiPathItem CreateSsePathItem(OpenApiDocument swaggerDoc)
    {
        return new OpenApiPathItem
        {
            Operations = new Dictionary<HttpMethod, OpenApiOperation>
            {
                [HttpMethod.Get] = new OpenApiOperation
                {
                    Tags = new HashSet<OpenApiTagReference> { new(Options.TagName, swaggerDoc, null) },
                    Summary = "Legacy SSE transport — open stream",
                    Description = """
                        Opens the Server-Sent Events channel for the legacy HTTP+SSE transport
                        ([MCP 2024-11-05 specification](https://modelcontextprotocol.io/specification/2024-11-05/basic/transports#http-with-sse)).

                        The server keeps this connection open and pushes `message` events.
                        Use the companion `POST {pattern}/message` endpoint to send requests.
                        """,
                    OperationId = "mcp-sse",
                    Responses = new OpenApiResponses
                    {
                        ["200"] = new OpenApiResponse
                        {
                            Description = "SSE stream — connection stays open until the client or server closes it.",
                            Content = new Dictionary<string, OpenApiMediaType>
                            {
                                ["text/event-stream"] = new OpenApiMediaType
                                {
                                    Schema = new OpenApiSchema { Type = JsonSchemaType.String, Description = "Server-Sent Events stream." }
                                }
                            }
                        }
                    }
                }
            }
        };
    }

    private OpenApiPathItem CreateSseMessagePathItem(OpenApiDocument swaggerDoc)
    {
        return new OpenApiPathItem
        {
            Operations = new Dictionary<HttpMethod, OpenApiOperation>
            {
                [HttpMethod.Post] = new OpenApiOperation
                {
                    Tags = new HashSet<OpenApiTagReference> { new(Options.TagName, swaggerDoc, null) },
                    Summary = "Legacy SSE transport — send message",
                    Description = """
                        Sends a JSON-RPC 2.0 request or notification through the legacy HTTP+SSE transport.
                        The server pushes the corresponding response via the SSE stream opened at `GET {pattern}/sse`.
                        """,
                    OperationId = "mcp-sse-message",
                    RequestBody = new OpenApiRequestBody
                    {
                        Required = true,
                        Description = "A JSON-RPC 2.0 request or notification.",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            ["application/json"] = new OpenApiMediaType
                            {
                                Schema = CreateJsonRpc2RequestSchema()
                            }
                        }
                    },
                    Responses = new OpenApiResponses
                    {
                        ["202"] = new OpenApiResponse
                        {
                            Description = "Accepted — the response will arrive via the SSE stream."
                        }
                    }
                }
            }
        };
    }

    private static OpenApiSchema CreateJsonRpc2RequestSchema()
    {
        return new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            Description = "JSON-RPC 2.0 request or notification envelope.",
            Properties = new Dictionary<string, IOpenApiSchema>
            {
                ["jsonrpc"] = new OpenApiSchema
                {
                    Type = JsonSchemaType.String,
                    Description = "Must be exactly \"2.0\".",
                    Enum = new List<JsonNode> { JsonValue.Create("2.0") }
                },
                ["id"] = new OpenApiSchema
                {
                    Type = JsonSchemaType.String | JsonSchemaType.Integer | JsonSchemaType.Null,
                    Description = "Request identifier. Omit for notifications."
                },
                ["method"] = new OpenApiSchema
                {
                    Type = JsonSchemaType.String,
                    Description = "The MCP method name, e.g. \"tools/list\" or \"tools/call\"."
                },
                ["params"] = new OpenApiSchema
                {
                    Type = JsonSchemaType.Object,
                    Description = "Method parameters. Shape depends on the method. Modern requests include protocol metadata in _meta.",
                    Properties = new Dictionary<string, IOpenApiSchema>
                    {
                        ["_meta"] = new OpenApiSchema
                        {
                            Type = JsonSchemaType.Object,
                            Description = "Modern per-request MCP metadata. Required for 2026-07-28 requests.",
                            Properties = new Dictionary<string, IOpenApiSchema>
                            {
                                ["io.modelcontextprotocol/protocolVersion"] = new OpenApiSchema
                                {
                                    Type = JsonSchemaType.String,
                                    Enum = [JsonValue.Create(McpDocumentFilter.ModernProtocolVersion)]
                                },
                                ["io.modelcontextprotocol/clientCapabilities"] = new OpenApiSchema
                                {
                                    Type = JsonSchemaType.Object
                                },
                                ["io.modelcontextprotocol/clientInfo"] = new OpenApiSchema
                                {
                                    Type = JsonSchemaType.Object
                                }
                            },
                            Required = new HashSet<string>
                            {
                                "io.modelcontextprotocol/protocolVersion",
                                "io.modelcontextprotocol/clientCapabilities"
                            }
                        }
                    }
                }
            },
            Required = new HashSet<string> { "jsonrpc", "method" }
        };
    }

    private static OpenApiSchema CreateJsonRpc2ResponseSchema()
    {
        return new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            Description = "JSON-RPC 2.0 response or error envelope.",
            Properties = new Dictionary<string, IOpenApiSchema>
            {
                ["jsonrpc"] = new OpenApiSchema
                {
                    Type = JsonSchemaType.String,
                    Description = "Always \"2.0\".",
                    Enum = new List<JsonNode> { JsonValue.Create("2.0") }
                },
                ["id"] = new OpenApiSchema
                {
                    Type = JsonSchemaType.String | JsonSchemaType.Integer | JsonSchemaType.Null,
                    Description = "Mirrors the id from the corresponding request."
                },
                ["result"] = new OpenApiSchema
                {
                    Type = JsonSchemaType.Object,
                    Description = "Present on success. Shape depends on the method.",
                    Properties = new Dictionary<string, IOpenApiSchema>
                    {
                        ["resultType"] = new OpenApiSchema
                        {
                            Type = JsonSchemaType.String,
                            Description = "Modern result discriminator.",
                            Enum = [JsonValue.Create("complete"), JsonValue.Create("input_required")]
                        }
                    }
                },
                ["error"] = new OpenApiSchema
                {
                    Type = JsonSchemaType.Object,
                    Description = "Present on failure.",
                    Properties = new Dictionary<string, IOpenApiSchema>
                    {
                        ["code"] = new OpenApiSchema { Type = JsonSchemaType.Integer, Description = "JSON-RPC 2.0 error code." },
                        ["message"] = new OpenApiSchema { Type = JsonSchemaType.String, Description = "Short error description." },
                        ["data"] = new OpenApiSchema { Type = JsonSchemaType.Object, Description = "Additional error details." }
                    }
                }
            }
        };
    }
}
