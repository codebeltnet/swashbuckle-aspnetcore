using System;
using Codebelt.Extensions.Xunit;
using ModelContextProtocol.AspNetCore;
using Swashbuckle.AspNetCore.SwaggerGen;
using Xunit;

namespace Codebelt.Extensions.Swashbuckle.AspNetCore.ModelContextProtocol;

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
    public void AddMcpServer_ShouldAddDocumentScopedMcpDocumentFilterDescriptor()
    {
        var options = new SwaggerGenOptions();

        options.AddMcpServer("legacy-mcp", mcp => mcp.Version = "2025-11-25");

        var descriptor = Assert.Single(options.DocumentFilterDescriptors);
        Assert.True(typeof(McpDocumentFilter).IsAssignableFrom(descriptor.Type));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void AddMcpServer_ShouldRejectMissingDocumentName(string documentName)
    {
        var options = new SwaggerGenOptions();

        Assert.ThrowsAny<ArgumentException>(() => options.AddMcpServer(documentName, _ => { }));
    }

    [Fact]
    public void AddMcpServer_ShouldApplyCustomOptions_WhenSetupIsProvided()
    {
        var customOptions = new McpDocumentOptions
        {
            Pattern = "/custom-mcp",
            TagName = "Custom",
            IncludeTools = false,
            SessionMode = HttpServerSessionMode.Stateful,
            EnableLegacySse = true,
            Version = "2025-11-25"
        };
        var filter = new McpDocumentFilter(customOptions);

        Assert.Equal("/custom-mcp", filter.Options.Pattern);
        Assert.Equal("Custom", filter.Options.TagName);
        Assert.False(filter.Options.IncludeTools);
        Assert.Equal(HttpServerSessionMode.Stateful, filter.Options.SessionMode);
        Assert.True(filter.Options.EnableLegacySse);
        Assert.Equal("2025-11-25", filter.Options.Version);
    }

    [Fact]
    public void AddMcpServer_ShouldUseDefaultOptions_WhenSetupIsNull()
    {
        var filter = new McpDocumentFilter(new McpDocumentOptions());

        Assert.Equal("/mcp", filter.Options.Pattern);
        Assert.Equal("MCP", filter.Options.TagName);
        Assert.True(filter.Options.IncludeTools);
        Assert.Equal(HttpServerSessionMode.Stateless, filter.Options.SessionMode);
        Assert.False(filter.Options.EnableLegacySse);
        Assert.Equal("2026-07-28", filter.Options.Version);
    }
}
