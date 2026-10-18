namespace Codebelt.Extensions.Swashbuckle.AspNetCore.ModelContextProtocol.Versions;

internal sealed class McpDocumentFilter20250618 : McpDocumentFilter
{
    internal McpDocumentFilter20250618(McpDocumentOptions options) : base(options, McpDocumentProtocolVersions.June2025)
    {
    }

    protected override bool SupportsProtocolVersionHeader => true;
}
