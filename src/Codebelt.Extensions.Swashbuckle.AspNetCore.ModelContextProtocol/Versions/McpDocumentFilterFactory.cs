using System;

namespace Codebelt.Extensions.Swashbuckle.AspNetCore.ModelContextProtocol.Versions;

internal static class McpDocumentFilterFactory
{
    internal static McpDocumentFilter Create(McpDocumentOptions options) => options.Version switch
    {
        McpDocumentProtocolVersions.July2026 => new McpDocumentFilter20260728(options),
        McpDocumentProtocolVersions.November2025 => new McpDocumentFilter20251125(options),
        McpDocumentProtocolVersions.June2025 => new McpDocumentFilter20250618(options),
        McpDocumentProtocolVersions.March2025 => new McpDocumentFilter20250326(options),
        McpDocumentProtocolVersions.November2024 => new McpDocumentFilter20241105(options),
        _ => throw new ArgumentOutOfRangeException(
            nameof(options),
            options.Version,
            $"The MCP protocol version must be one of: {string.Join(", ", McpDocumentProtocolVersions.Supported)}.")
    };
}
