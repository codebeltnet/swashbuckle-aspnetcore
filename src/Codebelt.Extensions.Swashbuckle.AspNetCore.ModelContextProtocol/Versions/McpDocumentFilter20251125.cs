namespace Codebelt.Extensions.Swashbuckle.AspNetCore.ModelContextProtocol.Versions;

internal sealed class McpDocumentFilter20251125 : McpDocumentFilter
{
    internal McpDocumentFilter20251125(McpDocumentOptions options) : base(options, McpDocumentProtocolVersions.November2025)
    {
    }

    protected override bool SupportsProtocolVersionHeader => true;

    protected override bool SupportsClientResponses => true;
}
