namespace Codebelt.Extensions.Swashbuckle.AspNetCore.ModelContextProtocol.Versions;

internal sealed class McpDocumentFilter20250326 : McpDocumentFilter
{
    internal McpDocumentFilter20250326(McpDocumentOptions options) : base(options, McpDocumentProtocolVersions.March2025)
    {
    }

    protected override bool SupportsBatchRequests => true;
}
