namespace Codebelt.Extensions.Swashbuckle.AspNetCore.ModelContextProtocol.Versions;

internal sealed class McpDocumentFilter20241105 : McpDocumentFilter
{
    internal McpDocumentFilter20241105(McpDocumentOptions options) : base(options, McpDocumentProtocolVersions.November2024)
    {
    }

    protected override bool UsesLegacySseTransport => true;
}
