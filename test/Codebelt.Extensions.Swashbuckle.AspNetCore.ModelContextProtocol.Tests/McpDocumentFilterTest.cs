using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading;
using Codebelt.Extensions.Xunit;
using Microsoft.OpenApi;
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
            var sut = new McpDocumentFilter(new McpDocumentOptions { EnableLegacySse = true });
            var doc = new OpenApiDocument();

            sut.Apply(doc, null);

            Assert.True(doc.Paths.ContainsKey("/mcp/sse"));
            Assert.True(doc.Paths.ContainsKey("/mcp/message"));
        }

        [Fact]
        public void Apply_ShouldUseCustomPattern_WhenPatternIsOverridden()
        {
            var sut = new McpDocumentFilter(new McpDocumentOptions { Pattern = "/ai/mcp", EnableLegacySse = true });
            var doc = new OpenApiDocument();

            sut.Apply(doc, null);

            Assert.True(doc.Paths.ContainsKey("/ai/mcp"));
            Assert.True(doc.Paths.ContainsKey("/ai/mcp/sse"));
            Assert.True(doc.Paths.ContainsKey("/ai/mcp/message"));
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
