using System.Collections.Generic;
using System.Linq;
using Codebelt.Extensions.Xunit;
using Microsoft.OpenApi;
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
    }
}
