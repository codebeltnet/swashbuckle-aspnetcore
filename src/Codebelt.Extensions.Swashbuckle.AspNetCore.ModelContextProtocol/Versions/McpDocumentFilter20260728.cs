namespace Codebelt.Extensions.Swashbuckle.AspNetCore.ModelContextProtocol.Versions;

internal sealed class McpDocumentFilter20260728 : McpDocumentFilter
{
    internal McpDocumentFilter20260728(McpDocumentOptions options) : base(options, McpDocumentProtocolVersions.July2026)
    {
    }

    protected override bool RequiresModernMetadata => true;

    protected override bool RequiresStandardHeaders => true;

    protected override bool SupportsProtocolVersionHeader => true;
}
