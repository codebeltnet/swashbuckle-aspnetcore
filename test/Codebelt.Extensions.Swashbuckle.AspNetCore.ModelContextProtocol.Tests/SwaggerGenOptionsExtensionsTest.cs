using System;
using Codebelt.Extensions.Xunit;
using Swashbuckle.AspNetCore.SwaggerGen;
using Xunit;

namespace Codebelt.Extensions.Swashbuckle.AspNetCore.ModelContextProtocol
{
    /// <summary>
    /// Unit tests for the <see cref="SwaggerGenOptionsExtensions"/> class.
    /// </summary>
    public class SwaggerGenOptionsExtensionsTest : Test
    {
        public SwaggerGenOptionsExtensionsTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void AddMcpServer_ShouldAddDocumentFilterDescriptor_WithDefaultOptions()
        {
            var options = new SwaggerGenOptions();

            options.AddMcpServer();

            Assert.Single(options.DocumentFilterDescriptors);
        }

        [Fact]
        public void AddMcpServer_ShouldAddMcpDocumentFilterDescriptor_WithDefaultOptions()
        {
            var options = new SwaggerGenOptions();

            options.AddMcpServer();

            Assert.Equal(typeof(McpDocumentFilter), options.DocumentFilterDescriptors[0].Type);
        }

        [Fact]
        public void AddMcpServer_ShouldApplyCustomOptions_WhenSetupIsProvided()
        {
            var customOptions = new McpDocumentOptions
            {
                Pattern = "/custom-mcp",
                TagName = "Custom",
                IncludeTools = false,
                EnableLegacySse = true
            };
            var filter = new McpDocumentFilter(customOptions);

            Assert.Equal("/custom-mcp", filter.Options.Pattern);
            Assert.Equal("Custom", filter.Options.TagName);
            Assert.False(filter.Options.IncludeTools);
            Assert.True(filter.Options.EnableLegacySse);
        }

        [Fact]
        public void AddMcpServer_ShouldUseDefaultOptions_WhenSetupIsNull()
        {
            var filter = new McpDocumentFilter(new McpDocumentOptions());

            Assert.Equal("/mcp", filter.Options.Pattern);
            Assert.Equal("MCP", filter.Options.TagName);
            Assert.True(filter.Options.IncludeTools);
            Assert.False(filter.Options.EnableLegacySse);
        }
    }
}
