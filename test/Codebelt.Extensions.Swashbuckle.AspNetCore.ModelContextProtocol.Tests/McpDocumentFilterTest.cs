using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading;
using System;
using Codebelt.Extensions.Xunit;
using Microsoft.OpenApi;
using ModelContextProtocol.AspNetCore;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Swashbuckle.AspNetCore.SwaggerGen;
using Xunit;

namespace Codebelt.Extensions.Swashbuckle.AspNetCore.ModelContextProtocol
{
    /// <summary>
    /// Unit tests for the <see cref="McpDocumentFilter"/> class.
    /// </summary>
    public class McpDocumentFilterTest : Test
    {
        public McpDocumentFilterTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void Apply_ShouldAddMcpTagToDocument_WithDefaultOptions()
        {
            var sut = new McpDocumentFilter(new McpDocumentOptions());
            var doc = new OpenApiDocument();

            sut.Apply(doc, null);

            Assert.NotNull(doc.Tags);
            Assert.Contains(doc.Tags, t => t.Name == "MCP");
        }

        [Fact]
        public void Apply_ShouldAddMcpTagDescription_WithDefaultOptions()
        {
            var sut = new McpDocumentFilter(new McpDocumentOptions());
            var doc = new OpenApiDocument();

            sut.Apply(doc, null);

            var tag = doc.Tags.Single(t => t.Name == "MCP");
            Assert.Contains("Model Context Protocol", tag.Description);
        }

        [Fact]
        public void Apply_ShouldNotDuplicateTag_WhenTagAlreadyExists()
        {
            var sut = new McpDocumentFilter(new McpDocumentOptions());
            var doc = new OpenApiDocument();
            doc.Tags = new HashSet<OpenApiTag> { new OpenApiTag { Name = "MCP", Description = "Existing" } };

            sut.Apply(doc, null);

            Assert.Equal(1, doc.Tags.Count(t => t.Name == "MCP"));
        }

        [Fact]
        public void Apply_ShouldAddStreamableHttpPath_WithDefaultOptions()
        {
            var sut = new McpDocumentFilter(new McpDocumentOptions());
            var doc = new OpenApiDocument();

            sut.Apply(doc, null);

            Assert.NotNull(doc.Paths);
            Assert.True(doc.Paths.ContainsKey("/mcp"));
        }

        [Fact]
        public void Apply_ShouldNotAddLegacySsePaths_WhenDisabled()
        {
            var sut = new McpDocumentFilter(new McpDocumentOptions { EnableLegacySse = false });
            var doc = new OpenApiDocument();

            sut.Apply(doc, null);

            Assert.False(doc.Paths.ContainsKey("/mcp/sse"));
            Assert.False(doc.Paths.ContainsKey("/mcp/message"));
        }

        [Fact]
        public void Apply_ShouldAddLegacySsePaths_WhenEnabled()
        {
            var sut = new McpDocumentFilter(new McpDocumentOptions { EnableLegacySse = true, SessionMode = HttpServerSessionMode.Stateful });
            var doc = new OpenApiDocument();

            sut.Apply(doc, null);

            Assert.True(doc.Paths.ContainsKey("/mcp/sse"));
            Assert.True(doc.Paths.ContainsKey("/mcp/message"));
        }

        [Fact]
        public void Apply_ShouldUseCustomPattern_WhenPatternIsOverridden()
        {
            var sut = new McpDocumentFilter(new McpDocumentOptions { Pattern = "/ai/mcp", EnableLegacySse = true, SessionMode = HttpServerSessionMode.Stateful });
            var doc = new OpenApiDocument();

            sut.Apply(doc, null);

            Assert.True(doc.Paths.ContainsKey("/ai/mcp"));
            Assert.True(doc.Paths.ContainsKey("/ai/mcp/sse"));
            Assert.True(doc.Paths.ContainsKey("/ai/mcp/message"));
        }

        [Fact]
        public void Apply_ShouldPreserveExistingPaths_WhenDocumentAlreadyHasPaths()
        {
            var sut = new McpDocumentFilter(new McpDocumentOptions());
            var doc = new OpenApiDocument
            {
                Paths = new OpenApiPaths
                {
                    ["/health"] = new OpenApiPathItem()
                }
            };

            sut.Apply(doc, null);

            Assert.Contains("/health", doc.Paths.Keys);
            Assert.Contains("/mcp", doc.Paths.Keys);
        }

        [Fact]
        public void Apply_ShouldUseCustomTagName_WhenTagNameIsOverridden()
        {
            var sut = new McpDocumentFilter(new McpDocumentOptions { TagName = "AI" });
            var doc = new OpenApiDocument();

            sut.Apply(doc, null);

            Assert.Contains(doc.Tags, t => t.Name == "AI");
            Assert.DoesNotContain(doc.Tags, t => t.Name == "MCP");
        }

        [Fact]
        public void Apply_ShouldAddPostOperationToStreamableHttpPath_WithDefaultOptions()
        {
            var sut = new McpDocumentFilter(new McpDocumentOptions());
            var doc = new OpenApiDocument();

            sut.Apply(doc, null);

            var pathItem = doc.Paths["/mcp"];
            Assert.Contains(pathItem.Operations, op => op.Key == System.Net.Http.HttpMethod.Post);
        }

        [Fact]
        public void Apply_ShouldExposeOnlyPostOperation_InStatelessMode()
        {
            var sut = new McpDocumentFilter(new McpDocumentOptions());
            var doc = new OpenApiDocument();

            sut.Apply(doc, null);

            Assert.Single(doc.Paths["/mcp"].Operations);
            Assert.Contains(System.Net.Http.HttpMethod.Post, doc.Paths["/mcp"].Operations.Keys);
        }

        [Theory]
        [InlineData(HttpServerSessionMode.Stateful)]
        [InlineData(HttpServerSessionMode.StatefulForInitializeClients)]
        public void Apply_ShouldExposeSessionOperations_InSessionModes(HttpServerSessionMode sessionMode)
        {
            var sut = new McpDocumentFilter(new McpDocumentOptions { SessionMode = sessionMode });
            var doc = new OpenApiDocument();

            sut.Apply(doc, null);

            Assert.Contains(System.Net.Http.HttpMethod.Get, doc.Paths["/mcp"].Operations.Keys);
            Assert.Contains(System.Net.Http.HttpMethod.Delete, doc.Paths["/mcp"].Operations.Keys);
        }

        [Theory]
        [InlineData(HttpServerSessionMode.Stateful)]
        [InlineData(HttpServerSessionMode.StatefulForInitializeClients)]
        public void Apply_ShouldDocumentSessionIdHeader_ForStatefulStreamableHttpOperations(HttpServerSessionMode sessionMode)
        {
            var sut = new McpDocumentFilter(new McpDocumentOptions { SessionMode = sessionMode });
            var doc = new OpenApiDocument();

            sut.Apply(doc, null);

            var pathItem = doc.Paths["/mcp"];
            var postParameters = pathItem.Operations[HttpMethod.Post].Parameters;
            var getParameters = pathItem.Operations[HttpMethod.Get].Parameters;
            var deleteParameters = pathItem.Operations[HttpMethod.Delete].Parameters;

            Assert.Contains(postParameters, p => p.Name == "Mcp-Session-Id" && !p.Required);
            Assert.Contains(getParameters, p => p.Name == "Mcp-Session-Id" && p.Required);
            Assert.Contains(deleteParameters, p => p.Name == "Mcp-Session-Id" && p.Required);
        }

        [Fact]
        public void Apply_ShouldRejectLegacySse_WhenStateless()
        {
            var sut = new McpDocumentFilter(new McpDocumentOptions { EnableLegacySse = true });

            var exception = Assert.Throws<InvalidOperationException>(() => sut.Apply(new OpenApiDocument(), null));

            Assert.Equal("Legacy SSE requires a stateful MCP HTTP session mode.", exception.Message);
        }

        [Fact]
        public void Apply_ShouldSetOperationId_ToMcp_ForStreamableHttpPath()
        {
            var sut = new McpDocumentFilter(new McpDocumentOptions());
            var doc = new OpenApiDocument();

            sut.Apply(doc, null);

            var operation = doc.Paths["/mcp"].Operations[System.Net.Http.HttpMethod.Post];
            Assert.Equal("mcp", operation.OperationId);
        }

        [Fact]
        public void Apply_ShouldInclude200And202Responses_ForStreamableHttpPath()
        {
            var sut = new McpDocumentFilter(new McpDocumentOptions());
            var doc = new OpenApiDocument();

            sut.Apply(doc, null);

            var responses = doc.Paths["/mcp"].Operations[System.Net.Http.HttpMethod.Post].Responses;
            Assert.True(responses.ContainsKey("200"));
            Assert.True(responses.ContainsKey("202"));
        }

        [Fact]
        public void Apply_ShouldDocumentModernProtocolHeadersAndMetadata()
        {
            var sut = new McpDocumentFilter(new McpDocumentOptions());
            var doc = new OpenApiDocument();

            sut.Apply(doc, null);

            var operation = doc.Paths["/mcp"].Operations[HttpMethod.Post];
            Assert.Contains(operation.Parameters, p => p.Name == "MCP-Protocol-Version" && p.Required);
            Assert.Contains(operation.Parameters, p => p.Name == "Mcp-Method" && p.Required);
            Assert.Contains(operation.Parameters, p => p.Name == "Mcp-Name");
            Assert.Contains("2026-07-28", operation.Description);
            Assert.Contains("server/discover", operation.Description);

            var requestSchema = Assert.IsType<OpenApiSchema>(operation.RequestBody.Content["application/json"].Schema);
            Assert.Contains("method", requestSchema.Required);
            Assert.Null(requestSchema.OneOf);
            var parametersSchema = Assert.IsType<OpenApiSchema>(requestSchema.Properties["params"]);
            var metadataSchema = Assert.IsType<OpenApiSchema>(parametersSchema.Properties["_meta"]);
            Assert.Contains("params", requestSchema.Required);
            Assert.Contains("_meta", parametersSchema.Required);
            Assert.Contains("io.modelcontextprotocol/protocolVersion", metadataSchema.Required);
            Assert.Contains("io.modelcontextprotocol/clientCapabilities", metadataSchema.Required);
        }

        [Theory]
        [InlineData("2026-07-28")]
        [InlineData("2025-11-25")]
        [InlineData("2025-06-18")]
        [InlineData("2025-03-26")]
        [InlineData("2024-11-05")]
        public void Apply_ShouldDocumentEverySupportedVersion(string protocolVersion)
        {
            var sut = new McpDocumentFilter(new McpDocumentOptions
            {
                IncludeTools = false,
                Version = protocolVersion
            });
            var doc = new OpenApiDocument();

            sut.Apply(doc, null);

            Assert.Contains(doc.Tags, tag => tag.Name == "MCP");
            if (protocolVersion == "2024-11-05")
            {
                Assert.DoesNotContain("/mcp", doc.Paths.Keys);
                Assert.Contains("/mcp/sse", doc.Paths.Keys);
                Assert.Contains("/mcp/message", doc.Paths.Keys);
            }
            else
            {
                var operation = doc.Paths["/mcp"].Operations[HttpMethod.Post];
                Assert.Contains(protocolVersion, operation.Description);
            }
        }

        [Theory]
        [InlineData("2025-11-25")]
        [InlineData("2025-06-18")]
        public void Apply_ShouldDocumentNegotiatedProtocolHeader_ForSessionBasedStreamableHttpVersions(string protocolVersion)
        {
            var sut = new McpDocumentFilter(new McpDocumentOptions
            {
                IncludeTools = false,
                Version = protocolVersion
            });
            var doc = new OpenApiDocument();

            sut.Apply(doc, null);

            var operation = doc.Paths["/mcp"].Operations[HttpMethod.Post];
            var protocolHeader = Assert.Single(operation.Parameters, parameter => parameter.Name == "MCP-Protocol-Version");
            Assert.False(protocolHeader.Required);
            var protocolHeaderSchema = Assert.IsType<OpenApiSchema>(protocolHeader.Schema);
            Assert.Equal(protocolVersion, Assert.Single(protocolHeaderSchema.Enum).GetValue<string>());
            Assert.DoesNotContain(operation.Parameters, parameter => parameter.Name == "Mcp-Method");
            Assert.DoesNotContain(operation.Parameters, parameter => parameter.Name == "Mcp-Name");

            var requestSchema = Assert.IsType<OpenApiSchema>(operation.RequestBody.Content["application/json"].Schema);
            var requestMessageSchema = Assert.IsType<OpenApiSchema>(requestSchema.OneOf[0]);
            var parametersSchema = Assert.IsType<OpenApiSchema>(requestMessageSchema.Properties["params"]);
            Assert.DoesNotContain("params", requestMessageSchema.Required);
            Assert.Null(parametersSchema.Required);
        }

        [Fact]
        public void Apply_ShouldDocumentBatchRequests_ForMarch2025StreamableHttp()
        {
            var sut = new McpDocumentFilter(new McpDocumentOptions
            {
                IncludeTools = false,
                Version = "2025-03-26"
            });
            var doc = new OpenApiDocument();

            sut.Apply(doc, null);

            var operation = doc.Paths["/mcp"].Operations[HttpMethod.Post];
            Assert.DoesNotContain(operation.Parameters, parameter => parameter.Name == "MCP-Protocol-Version");
            Assert.DoesNotContain(operation.Parameters, parameter => parameter.Name == "Mcp-Method");
            Assert.DoesNotContain(operation.Parameters, parameter => parameter.Name == "Mcp-Name");
            Assert.Contains("batch array", operation.RequestBody.Description);

            var requestSchema = Assert.IsType<OpenApiSchema>(operation.RequestBody.Content["application/json"].Schema);
            Assert.Equal(2, requestSchema.OneOf.Count);
            var singleMessageSchema = Assert.IsType<OpenApiSchema>(requestSchema.OneOf[0]);
            AssertJsonRpcRequestAndResponseSchema(singleMessageSchema);
            var batchSchema = Assert.IsType<OpenApiSchema>(requestSchema.OneOf[1]);
            Assert.Equal(JsonSchemaType.Array, batchSchema.Type);
            Assert.NotNull(batchSchema.Items);
            AssertJsonRpcRequestAndResponseSchema(Assert.IsType<OpenApiSchema>(batchSchema.Items));
        }

        [Theory]
        [InlineData("2025-11-25")]
        [InlineData("2025-06-18")]
        [InlineData("2025-03-26")]
        public void Apply_ShouldDocumentClientResponses_For2025StreamableHttpVersions(string protocolVersion)
        {
            var sut = new McpDocumentFilter(new McpDocumentOptions
            {
                IncludeTools = false,
                Version = protocolVersion
            });
            var doc = new OpenApiDocument();

            sut.Apply(doc, null);

            var operation = doc.Paths["/mcp"].Operations[HttpMethod.Post];
            var requestSchema = Assert.IsType<OpenApiSchema>(operation.RequestBody.Content["application/json"].Schema);
            var singleMessageSchema = protocolVersion == "2025-03-26"
                ? Assert.IsType<OpenApiSchema>(requestSchema.OneOf[0])
                : requestSchema;

            AssertJsonRpcRequestAndResponseSchema(singleMessageSchema);
        }

        [Fact]
        public void Apply_ShouldKeepLegacyExamplesFreeOfModernMetadata()
        {
            var sut = new McpDocumentFilter(new McpDocumentOptions
            {
                Version = "2025-11-25"
            });
            var doc = new OpenApiDocument();

            sut.Apply(doc, null);

            var request = doc.Paths["/mcp"].Operations[HttpMethod.Post].RequestBody.Content["application/json"];
            var example = Assert.IsType<OpenApiExample>(request.Examples["snake_case_tool"]);
            var value = Assert.IsType<JsonObject>(example.Value);
            var parameters = Assert.IsType<JsonObject>(value["params"]);

            Assert.Null(parameters["_meta"]);
        }

        [Fact]
        public void Apply_ShouldDocumentLegacyHttpSseTransport_ForNovember2024()
        {
            var sut = new McpDocumentFilter(new McpDocumentOptions
            {
                IncludeTools = false,
                Version = "2024-11-05"
            });
            var doc = new OpenApiDocument();

            sut.Apply(doc, null);

            Assert.False(doc.Paths.ContainsKey("/mcp"));
            Assert.Equal("mcp-sse", doc.Paths["/mcp/sse"].Operations[HttpMethod.Get].OperationId);
            Assert.Equal("mcp-sse-message", doc.Paths["/mcp/message"].Operations[HttpMethod.Post].OperationId);
            Assert.Contains("2024-11-05", doc.Paths["/mcp/sse"].Operations[HttpMethod.Get].Description);

            var messageRequestSchema = Assert.IsType<OpenApiSchema>(doc.Paths["/mcp/message"].Operations[HttpMethod.Post].RequestBody.Content["application/json"].Schema);
            Assert.Contains("method", messageRequestSchema.Required);
            Assert.Null(messageRequestSchema.OneOf);
        }

        [Fact]
        public void Apply_ShouldRejectUnsupportedProtocolVersion()
        {
            var sut = new McpDocumentFilter(new McpDocumentOptions { Version = "unsupported" });

            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => sut.Apply(new OpenApiDocument(), null));

            Assert.Equal("options", exception.ParamName);
            Assert.Contains("2026-07-28", exception.Message);
            Assert.Contains("2024-11-05", exception.Message);
        }

        [Fact]
        public void Apply_ShouldOmitToolExamplesAndTable_WhenToolDiscoveryIsDisabled()
        {
            var sut = new McpDocumentFilter(new McpDocumentOptions { IncludeTools = false });
            var doc = new OpenApiDocument();

            sut.Apply(doc, null);

            var operation = doc.Paths["/mcp"].Operations[HttpMethod.Post];
            var request = operation.RequestBody.Content["application/json"];

            Assert.Null(request.Examples);
            Assert.DoesNotContain("Available tools", operation.Description);
        }

        [Fact]
        public void Apply_ShouldKeepLegacySseRequestParametersOptional()
        {
            var sut = new McpDocumentFilter(new McpDocumentOptions
            {
                EnableLegacySse = true,
                SessionMode = HttpServerSessionMode.Stateful
            });
            var doc = new OpenApiDocument();

            sut.Apply(doc, null);

            var requestSchema = Assert.IsType<OpenApiSchema>(doc.Paths["/mcp/message"].Operations[HttpMethod.Post]
                .RequestBody.Content["application/json"].Schema);
            var parametersSchema = Assert.IsType<OpenApiSchema>(requestSchema.Properties["params"]);

            Assert.DoesNotContain("params", requestSchema.Required);
            Assert.Null(parametersSchema.Required);
        }

        [Fact]
        public void Apply_ShouldPreserveExistingTags_WhenDocumentAlreadyHasTags()
        {
            var sut = new McpDocumentFilter(new McpDocumentOptions());
            var doc = new OpenApiDocument();
            doc.Tags = new HashSet<OpenApiTag> { new OpenApiTag { Name = "Existing" } };

            sut.Apply(doc, null);

            Assert.Contains(doc.Tags, t => t.Name == "Existing");
            Assert.Contains(doc.Tags, t => t.Name == "MCP");
        }

        [Fact]
        public void Apply_ShouldDiscoverAttributedTools_AndPopulateExamplesAndDescriptions()
        {
            var sut = new McpDocumentFilter(new McpDocumentOptions());
            var doc = new OpenApiDocument();

            sut.Apply(doc, null);

            var operation = doc.Paths["/mcp"].Operations[HttpMethod.Post];
            var request = operation.RequestBody.Content["application/json"];

            Assert.NotNull(request.Examples);
            Assert.Contains("snake_case_tool", request.Examples.Keys);
            Assert.Contains("custom_tool", request.Examples.Keys);
            Assert.Contains("Available tools", operation.Description);
            Assert.Contains("| `snake_case_tool` |", operation.Description);
            Assert.Contains("| `custom_tool` |", operation.Description);
            Assert.Contains("`message` (string)", operation.Description);
            Assert.Contains("`enabled` (boolean)", operation.Description);
            Assert.Contains("`count` (integer)", operation.Description);
            Assert.Contains("`ratio` (number)", operation.Description);
            Assert.Contains("`payload` (ToolPayload)", operation.Description);
            Assert.DoesNotContain("cancellationToken", operation.Description);
            Assert.DoesNotContain("rpcMessage", operation.Description);

            var snakeCaseExample = Assert.IsType<OpenApiExample>(request.Examples["snake_case_tool"]);
            var snakeCaseValue = Assert.IsType<JsonObject>(snakeCaseExample.Value);
            var snakeCaseParameters = Assert.IsType<JsonObject>(snakeCaseValue["params"]);
            var snakeCaseArguments = Assert.IsType<JsonObject>(snakeCaseParameters["arguments"]);

            Assert.Equal("2.0", snakeCaseValue["jsonrpc"]!.GetValue<string>());
            Assert.Equal(1, snakeCaseValue["id"]!.GetValue<int>());
            Assert.Equal("tools/call", snakeCaseValue["method"]!.GetValue<string>());
            Assert.Equal("snake_case_tool", snakeCaseParameters["name"]!.GetValue<string>());
            Assert.Equal(string.Empty, snakeCaseArguments["message"]!.GetValue<string>());
            Assert.False(snakeCaseArguments["enabled"]!.GetValue<bool>());
            Assert.Equal(0, snakeCaseArguments["count"]!.GetValue<int>());
            Assert.Equal(0.0d, snakeCaseArguments["ratio"]!.GetValue<double>());
            Assert.IsType<JsonObject>(snakeCaseArguments["payload"]);
            Assert.DoesNotContain("cancellationToken", snakeCaseArguments.Select(argument => argument.Key));
            Assert.DoesNotContain("rpcMessage", snakeCaseArguments.Select(argument => argument.Key));

            var customExample = Assert.IsType<OpenApiExample>(request.Examples["custom_tool"]);
            var customValue = Assert.IsType<JsonObject>(customExample.Value);
            var customParameters = Assert.IsType<JsonObject>(customValue["params"]);

            Assert.Equal("custom_tool", customParameters["name"]!.GetValue<string>());
            Assert.Null(customParameters["arguments"]);
        }

        [Fact]
        public void Apply_ShouldUseToolNameAsFallbackDescription_WhenDescriptionAttributeIsMissing()
        {
            var sut = new McpDocumentFilter(new McpDocumentOptions());
            var doc = new OpenApiDocument();

            sut.Apply(doc, null);

            var operation = doc.Paths["/mcp"].Operations[HttpMethod.Post];

            Assert.Contains("| `custom_tool` |", operation.Description);
            Assert.Contains("| `custom_tool` | — | custom_tool |", operation.Description);
        }

        private static void AssertJsonRpcRequestAndResponseSchema(OpenApiSchema schema)
        {
            Assert.NotNull(schema.OneOf);
            Assert.Equal(2, schema.OneOf.Count);

            var requestSchema = Assert.IsType<OpenApiSchema>(schema.OneOf[0]);
            Assert.Contains("jsonrpc", requestSchema.Required);
            Assert.Contains("method", requestSchema.Required);

            var responseSchema = Assert.IsType<OpenApiSchema>(schema.OneOf[1]);
            Assert.Contains("jsonrpc", responseSchema.Required);
            Assert.Contains("id", responseSchema.Required);
            Assert.Contains("method", Assert.IsType<OpenApiSchema>(responseSchema.Not).Required);
            Assert.Equal(2, responseSchema.OneOf.Count);
            Assert.Contains("result", Assert.IsType<OpenApiSchema>(responseSchema.OneOf[0]).Required);
            Assert.Contains("error", Assert.IsType<OpenApiSchema>(responseSchema.OneOf[1]).Required);
        }

        [McpServerToolType]
        public class FakeToolContainer
        {
            [Description("Discovers parameter metadata and example values.")]
            [McpServerTool]
            public string SnakeCaseTool(string message, bool enabled, int count, double ratio, ToolPayload payload, CancellationToken cancellationToken, JsonRpcMessage rpcMessage)
            {
                return message;
            }

            [McpServerTool(Name = "custom_tool")]
            public string NamedTool()
            {
                return "ok";
            }
        }

        public class ToolPayload
        {
            public string Value { get; set; }
        }
    }
}
