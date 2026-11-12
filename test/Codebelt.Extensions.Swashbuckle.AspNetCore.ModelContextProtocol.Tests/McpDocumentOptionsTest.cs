using System;
using Codebelt.Extensions.Xunit;
using Cuemon;
using ModelContextProtocol.AspNetCore;
using Xunit;

namespace Codebelt.Extensions.Swashbuckle.AspNetCore.ModelContextProtocol;

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
        Assert.Equal(HttpServerSessionMode.Stateless, sut.SessionMode);
        Assert.False(sut.EnableLegacySse);
        Assert.Equal("2026-07-28", sut.Version);
    }

    [Fact]
    public void McpDocumentOptions_ShouldAllowCustomValues()
    {
        var sut = new McpDocumentOptions
        {
            Pattern = "/ai",
            TagName = "AI",
            IncludeTools = false,
            SessionMode = HttpServerSessionMode.Stateful,
            EnableLegacySse = true,
            Version = "2025-11-25"
        };

        Assert.Equal("/ai", sut.Pattern);
        Assert.Equal("AI", sut.TagName);
        Assert.False(sut.IncludeTools);
        Assert.Equal(HttpServerSessionMode.Stateful, sut.SessionMode);
        Assert.True(sut.EnableLegacySse);
        Assert.Equal("2025-11-25", sut.Version);
    }

    [Theory]
    [InlineData("2026-07-28")]
    [InlineData("2025-11-25")]
    [InlineData("2025-06-18")]
    [InlineData("2025-03-26")]
    [InlineData("2024-11-05")]
    public void ValidateOptions_ShouldAcceptSupportedProtocolVersions(string protocolVersion)
    {
        var sut = new McpDocumentOptions
        {
            Version = protocolVersion
        };

        sut.ValidateOptions();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ValidateOptions_ShouldRejectMissingProtocolVersion(string protocolVersion)
    {
        var sut = new McpDocumentOptions
        {
            Version = protocolVersion
        };

        Assert.Throws<InvalidOperationException>(() => sut.ValidateOptions());
    }

    [Theory]
    [InlineData("2026-07-29")]
    [InlineData("2025-11-24")]
    [InlineData("2025-06-17")]
    [InlineData("2025-03-25")]
    [InlineData("2024-11-04")]
    [InlineData("unsupported")]
    public void ValidateOptions_ShouldRejectUnsupportedProtocolVersion(string protocolVersion)
    {
        var sut = new McpDocumentOptions
        {
            Version = protocolVersion
        };

        var ix = Assert.Throws<InvalidOperationException>(() => sut.ValidateOptions());
        TestOutput.WriteLine(ix.Message);
        var ax = Assert.Throws<ArgumentException>(() => Validator.ThrowIfInvalidOptions(sut));
        TestOutput.WriteLine(ax.ToString());
    }
}
