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

namespace Codebelt.Extensions.Swashbuckle.AspNetCore.ModelContextProtocol.Versions;

internal abstract class McpDocumentFilter
{
    protected McpDocumentFilter(McpDocumentOptions options, string version)
    {
        Options = options;
        Version = version;
    }

    protected McpDocumentOptions Options { get; }

    protected string Version { get; }

    protected virtual bool UsesLegacySseTransport => false;

    protected virtual bool RequiresModernMetadata => false;

    protected virtual bool RequiresStandardHeaders => false;

    protected virtual bool SupportsProtocolVersionHeader => false;

    protected virtual bool SupportsClientResponses => false;

    protected virtual bool SupportsBatchRequests => false;

    internal void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        _ = context;

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

        var tools = Options.IncludeTools ? DiscoverTools() : null;

        swaggerDoc.Paths ??= new OpenApiPaths();
        if (UsesLegacySseTransport)
        {
            swaggerDoc.Paths.Add(Options.Pattern + "/sse", CreateSsePathItem(swaggerDoc));
            swaggerDoc.Paths.Add(Options.Pattern + "/message", CreateSseMessagePathItem(swaggerDoc));
            return;
        }

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
                    .Select(p => (ParamName: p.Name!, p.ParameterType))
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

    private JsonObject BuildExampleBody(McpToolInfo tool)
    {
        var paramsObject = new JsonObject { ["name"] = JsonValue.Create(tool.Name) };
        if (tool.Parameters.Count > 0)
        {
            var arguments = new JsonObject();
            foreach (var (paramName, paramType) in tool.Parameters)
                arguments[paramName] = GetDefaultExampleValue(paramType);
            paramsObject["arguments"] = arguments;
        }

        if (RequiresModernMetadata)
        {
            paramsObject["_meta"] = new JsonObject
            {
                ["io.modelcontextprotocol/protocolVersion"] = JsonValue.Create(Version),
                ["io.modelcontextprotocol/clientCapabilities"] = new JsonObject()
            };
        }

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

    private string BuildStreamableHttpDescription(IReadOnlyList<McpToolInfo> tools)
    {
        var requestShape = SupportsBatchRequests
            ? "one JSON-RPC 2.0 request, notification, response, or batch array"
            : "one JSON-RPC 2.0 request, notification, or response object";

        string description;
        if (RequiresModernMetadata)
        {
            description = $"""
                Bidirectional JSON-RPC 2.0 endpoint following the
                [MCP Streamable HTTP specification ({Version})](https://modelcontextprotocol.io/specification/{Version}/basic/transports/streamable-http).

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
        }
        else
        {
            var protocolHeaderDescription = SupportsProtocolVersionHeader
                ? "Clients send `MCP-Protocol-Version` after the initialize handshake."
                : "Protocol version negotiation is performed by the initialize handshake.";

            description = $"""
                Bidirectional JSON-RPC 2.0 endpoint following the
                [MCP Streamable HTTP specification ({Version})](https://modelcontextprotocol.io/specification/{Version}/basic/transports/streamable-http).

                **Request** — send {requestShape} as an HTTP `POST`. {protocolHeaderDescription} The server and client
                complete the `initialize` and `notifications/initialized` handshake before ordinary requests.

                **Response** — the server returns either `application/json` for a complete response or
                `text/event-stream` for a streaming response. A `202 Accepted` with an empty body is returned for
                accepted notifications. Stateful and hybrid modes additionally expose `GET` and `DELETE` for session
                clients. Legacy SSE is a separate, opt-in compatibility transport.
                """;
        }

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

    private OpenApiPathItem CreateStreamableHttpPathItem(OpenApiDocument swaggerDoc, IReadOnlyList<McpToolInfo> tools)
    {
        var requestMediaType = new OpenApiMediaType
        {
            Schema = CreateJsonRpc2RequestSchema(
                requireModernMetadata: RequiresModernMetadata,
                allowBatch: SupportsBatchRequests,
                allowResponses: SupportsClientResponses)
        };
        if (tools?.Count > 0)
            requestMediaType.Examples = tools.ToDictionary(
                t => t.Name,
                t => (IOpenApiExample)new OpenApiExample { Summary = t.Name, Value = BuildExampleBody(t) }
            );

        var parameters = CreateHttpParameters();
        if (Options.SessionMode != HttpServerSessionMode.Stateless)
            parameters.Add(CreateSessionIdParameter(required: false));

        return new OpenApiPathItem
        {
            Operations = new Dictionary<HttpMethod, OpenApiOperation>
            {
                [HttpMethod.Post] = new OpenApiOperation
                {
                    Tags = new HashSet<OpenApiTagReference> { new(Options.TagName, swaggerDoc, null) },
                    Summary = "Streamable HTTP",
                    Description = BuildStreamableHttpDescription(tools),
                    OperationId = "mcp",
                    Parameters = parameters,
                    RequestBody = new OpenApiRequestBody
                    {
                        Required = true,
                        Description = CreateRequestBodyDescription(),
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

    private string CreateRequestBodyDescription()
    {
        if (RequiresModernMetadata)
            return "A single JSON-RPC 2.0 request or notification object. Modern Streamable HTTP does not accept batch arrays.";
        if (SupportsBatchRequests)
            return "A single JSON-RPC 2.0 request, notification, or response object, or a JSON-RPC 2.0 batch array.";
        return "A single JSON-RPC 2.0 request, notification, or response object.";
    }

    private List<IOpenApiParameter> CreateHttpParameters()
    {
        var parameters = new List<IOpenApiParameter>
        {
            new OpenApiParameter
            {
                Name = "Accept",
                In = ParameterLocation.Header,
                Required = true,
                Description = "Must list both application/json and text/event-stream.",
                Schema = new OpenApiSchema { Type = JsonSchemaType.String }
            }
        };

        if (SupportsProtocolVersionHeader)
        {
            parameters.Add(new OpenApiParameter
            {
                Name = "MCP-Protocol-Version",
                In = ParameterLocation.Header,
                Required = RequiresModernMetadata,
                Description = RequiresModernMetadata
                    ? "Modern protocol version. Must match params._meta.io.modelcontextprotocol/protocolVersion."
                    : "Required on requests after initialize. Must match the negotiated MCP protocol version.",
                Schema = new OpenApiSchema
                {
                    Type = JsonSchemaType.String,
                    Enum = [JsonValue.Create(Version)]
                }
            });
        }

        if (RequiresStandardHeaders)
        {
            parameters.Add(new OpenApiParameter
            {
                Name = "Mcp-Method",
                In = ParameterLocation.Header,
                Required = true,
                Description = "Must match the JSON-RPC method.",
                Schema = new OpenApiSchema { Type = JsonSchemaType.String }
            });
            parameters.Add(new OpenApiParameter
            {
                Name = "Mcp-Name",
                In = ParameterLocation.Header,
                Description = "Required for tools/call, resources/read, and prompts/get; must match the name or URI in params.",
                Schema = new OpenApiSchema { Type = JsonSchemaType.String }
            });
        }

        return parameters;
    }

    private static OpenApiParameter CreateSessionIdParameter(bool required) => new()
    {
        Name = "Mcp-Session-Id",
        In = ParameterLocation.Header,
        Required = required,
        Description = "The session identifier returned in the initialize response. Omit it only from the initialize request.",
        Schema = new OpenApiSchema { Type = JsonSchemaType.String }
    };

    private string CreateSessionGetDescription() => RequiresModernMetadata
        ? "Opens the optional session event stream for stateful initialize-handshake clients. It is not mapped in stateless mode and returns `405 Method Not Allowed` for modern 2026-07-28 requests."
        : $"Opens the optional session event stream for stateful initialize-handshake clients using MCP {Version}. It is not mapped in stateless mode and may return `405 Method Not Allowed` when the selected transport does not support session streams.";

    private string CreateSessionDeleteDescription() => RequiresModernMetadata
        ? "Terminates a stateful initialize-handshake session. It is not mapped in stateless mode and returns `405 Method Not Allowed` for modern 2026-07-28 requests."
        : $"Terminates a stateful initialize-handshake session for MCP {Version}. It is not mapped in stateless mode and may return `405 Method Not Allowed` when sessions are unavailable.";

    private string CreateSessionMethodNotAllowedDescription() => RequiresModernMetadata
        ? "Method Not Allowed for stateless or modern protocol requests."
        : "Method Not Allowed for stateless or unsupported session requests.";

    private OpenApiOperation CreateSessionGetOperation(OpenApiDocument swaggerDoc) => new()
    {
        Tags = new HashSet<OpenApiTagReference> { new(Options.TagName, swaggerDoc, null) },
        Summary = "Streamable HTTP — session event stream",
        Description = CreateSessionGetDescription(),
        OperationId = "mcp-session-stream",
        Parameters = new List<IOpenApiParameter> { CreateSessionIdParameter(required: true) },
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
            ["405"] = new OpenApiResponse { Description = CreateSessionMethodNotAllowedDescription() }
        }
    };

    private OpenApiOperation CreateSessionDeleteOperation(OpenApiDocument swaggerDoc) => new()
    {
        Tags = new HashSet<OpenApiTagReference> { new(Options.TagName, swaggerDoc, null) },
        Summary = "Streamable HTTP — terminate session",
        Description = CreateSessionDeleteDescription(),
        OperationId = "mcp-session-delete",
        Parameters = new List<IOpenApiParameter> { CreateSessionIdParameter(required: true) },
        Responses = new OpenApiResponses
        {
            ["200"] = new OpenApiResponse { Description = "Session terminated." },
            ["404"] = new OpenApiResponse { Description = "Session not found." },
            ["405"] = new OpenApiResponse { Description = CreateSessionMethodNotAllowedDescription() }
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
                        ([MCP 2024-11-05 specification](https://modelcontextprotocol.io/specification/2024-11-05/basic/transports#http-with-sse).

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

    private OpenApiSchema CreateJsonRpc2RequestSchema(bool requireModernMetadata = false, bool allowBatch = false, bool allowResponses = false)
    {
        var metadataSchema = new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            Description = requireModernMetadata ? "Modern per-request MCP metadata. Required for 2026-07-28 requests." : "Optional JSON-RPC request metadata."
        };

        if (requireModernMetadata)
        {
            metadataSchema.Properties = new Dictionary<string, IOpenApiSchema>
            {
                ["io.modelcontextprotocol/protocolVersion"] = new OpenApiSchema
                {
                    Type = JsonSchemaType.String,
                    Enum = [JsonValue.Create(Version)]
                },
                ["io.modelcontextprotocol/clientCapabilities"] = new OpenApiSchema
                {
                    Type = JsonSchemaType.Object
                },
                ["io.modelcontextprotocol/clientInfo"] = new OpenApiSchema
                {
                    Type = JsonSchemaType.Object
                }
            };
            metadataSchema.Required = new HashSet<string>
            {
                "io.modelcontextprotocol/protocolVersion",
                "io.modelcontextprotocol/clientCapabilities"
            };
        }

        var schema = new OpenApiSchema
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
                    Description = requireModernMetadata ? "Method parameters. Shape depends on the method. Modern requests include protocol metadata in _meta." : "Optional method parameters and metadata.",
                    Properties = new Dictionary<string, IOpenApiSchema>
                    {
                        ["_meta"] = metadataSchema
                    }
                }
            },
            Required = new HashSet<string> { "jsonrpc", "method" }
        };

        if (requireModernMetadata)
        {
            schema.Required.Add("params");
            var parametersSchema = (OpenApiSchema)schema.Properties["params"];
            parametersSchema.Required = new HashSet<string> { "_meta" };
        }

        var messageSchema = allowResponses
            ? new OpenApiSchema
            {
                Description = "A JSON-RPC 2.0 request, notification, or response object.",
                OneOf = new List<IOpenApiSchema> { schema, CreateJsonRpc2ResponseSchema() }
            }
            : schema;

        if (allowBatch)
        {
            var batchItemSchema = CreateJsonRpc2RequestSchema(allowResponses: allowResponses);
            return new OpenApiSchema
            {
                Description = "A JSON-RPC 2.0 request, notification, response, or batch array.",
                OneOf = new List<IOpenApiSchema>
                {
                    messageSchema,
                    new OpenApiSchema
                    {
                        Type = JsonSchemaType.Array,
                        Items = batchItemSchema,
                        Description = "A non-empty JSON-RPC 2.0 batch array."
                    }
                }
            };
        }

        return messageSchema;
    }

    private OpenApiSchema CreateJsonRpc2ResponseSchema()
    {
        var resultProperties = new Dictionary<string, IOpenApiSchema>();
        if (RequiresModernMetadata)
        {
            resultProperties["resultType"] = new OpenApiSchema
            {
                Type = JsonSchemaType.String,
                Description = "Modern result discriminator.",
                Enum = [JsonValue.Create("complete"), JsonValue.Create("input_required")]
            };
        }

        return new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            Description = "JSON-RPC 2.0 response or error envelope.",
            Required = new HashSet<string> { "jsonrpc", "id" },
            Not = new OpenApiSchema
            {
                Required = new HashSet<string> { "method" }
            },
            OneOf = new List<IOpenApiSchema>
            {
                new OpenApiSchema { Required = new HashSet<string> { "result" } },
                new OpenApiSchema { Required = new HashSet<string> { "error" } }
            },
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
                    Properties = resultProperties
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

    private sealed record McpToolInfo(string Name, string Description, IReadOnlyList<(string ParamName, Type ParamType)> Parameters);
}
