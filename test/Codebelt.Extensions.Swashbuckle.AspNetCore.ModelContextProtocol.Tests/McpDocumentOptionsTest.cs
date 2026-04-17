using Codebelt.Extensions.Xunit;
using Xunit;

namespace Codebelt.Extensions.Swashbuckle.AspNetCore.ModelContextProtocol
{
    /// <summary>
    /// Unit tests for the <see cref="McpDocumentOptions"/> class.
    /// </summary>
    public class McpDocumentOptionsTest : Test
    {
        public McpDocumentOptionsTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void McpDocumentOptions_ShouldHaveDefaultValues()
        {
            var sut = new McpDocumentOptions();

            Assert.Equal("/mcp", sut.Pattern);
            Assert.Equal("MCP", sut.TagName);
            Assert.True(sut.IncludeTools);
            Assert.False(sut.EnableLegacySse);
        }

        [Fact]
        public void McpDocumentOptions_ShouldAllowCustomValues()
        {
            var sut = new McpDocumentOptions
            {
                Pattern = "/ai",
                TagName = "AI",
                IncludeTools = false,
                EnableLegacySse = true
            };

            Assert.Equal("/ai", sut.Pattern);
            Assert.Equal("AI", sut.TagName);
            Assert.False(sut.IncludeTools);
            Assert.True(sut.EnableLegacySse);
        }
    }
}
