using System;
using System.Linq;
using Codebelt.Extensions.Swashbuckle.AspNetCore.ModelContextProtocol.Versions;
using Cuemon;
using Cuemon.Configuration;
using ModelContextProtocol.AspNetCore;

namespace Codebelt.Extensions.Swashbuckle.AspNetCore.ModelContextProtocol;


/// <summary>
/// Provides programmatic configuration for the <see cref="McpDocumentFilter"/> class.
/// </summary>
public class McpDocumentOptions : IValidatableParameterObject
{

    /// <summary>
    /// Initializes a new instance of the <see cref="McpDocumentOptions"/> class.
    /// </summary>
    /// <remarks>
    /// The following table shows the initial property values for an instance of <see cref="McpDocumentOptions"/>.
    /// <list type="table">
    /// <listheader><term>Property</term><description>Initial value</description></listheader>
    /// <item><term><see cref="Pattern"/></term><description><c>/mcp</c></description></item>
    /// <item><term><see cref="TagName"/></term><description><c>MCP</c></description></item>
    /// <item><term><see cref="IncludeTools"/></term><description><c>true</c></description></item>
    /// <item><term><see cref="SessionMode"/></term><description><see cref="HttpServerSessionMode.Stateless"/></description></item>
    /// <item><term><see cref="EnableLegacySse"/></term><description><c>false</c></description></item>
    /// <item><term><see cref="Version"/></term><description><c>2026-07-28</c></description></item>
    /// </list>
    /// </remarks>
    public McpDocumentOptions()
    {
        Pattern = "/mcp";
        TagName = "MCP";
        IncludeTools = true;
        SessionMode = HttpServerSessionMode.Stateless;
        EnableLegacySse = false;
        Version = McpDocumentProtocolVersions.July2026;
    }


    /// <summary>
    /// Gets or sets the route pattern prefix that was passed to <c>MapMcp</c>.
    /// </summary>
    /// <value>Defaults to <c>/mcp</c>.</value>
    public string Pattern { get; set; }


    /// <summary>
    /// Gets or sets the OpenAPI tag name used to group the MCP endpoints.
    /// </summary>
    /// <value>Defaults to <c>MCP</c>.</value>
    public string TagName { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether individual MCP tools should be documented as named examples on the <c>POST {Pattern}</c> request body.
    /// </summary>
    /// <value>Defaults to <c>true</c>.</value>
    public bool IncludeTools { get; set; }


    /// <summary>
    /// Gets or sets the MCP protocol revision documented by <see cref="McpDocumentFilter"/>.
    /// </summary>
    /// <value>
    /// Defaults to <c>2026-07-28</c>. The supported revisions are
    /// <c>2026-07-28</c>, <c>2025-11-25</c>, <c>2025-06-18</c>, <c>2025-03-26</c>, and <c>2024-11-05</c>.
    /// </value>
    /// <remarks>
    /// The selected revision changes the documented transport, request metadata, headers, and request schema.
    /// The 2024-11-05 revision documents the legacy HTTP+SSE <c>/sse</c> and <c>/message</c> endpoints.
    /// </remarks>
    public string Version { get; set; }


    /// <summary>
    /// Gets or sets the Streamable HTTP session mode used by <c>MapMcp</c>.
    /// </summary>
    /// <value>Defaults to <see cref="HttpServerSessionMode.Stateless"/>, the modern MCP-compatible mode.</value>
    /// <remarks>
    /// Keep this value aligned with <see cref="HttpServerTransportOptions.SessionMode"/>. Stateless mode exposes
    /// only the POST endpoint. Stateful and hybrid modes also expose the session GET and DELETE endpoints for
    /// initialize-handshake clients. The <see cref="EnableLegacySse"/> option requires a non-stateless mode.
    /// </remarks>
    public HttpServerSessionMode SessionMode { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether the legacy SSE transport endpoints
    /// (<c>{Pattern}/sse</c> and <c>{Pattern}/message</c>) should also be documented.
    /// </summary>
    /// <value>Defaults to <c>false</c>.</value>
    public bool EnableLegacySse { get; set; }

    /// <summary>
    /// Determines whether the public read-write properties of this instance are in a valid state.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// <see cref="Version"/> is null, empty, or consists only of white-space characters - or -
    /// <see cref="Version"/> is not one of the supported MCP revisions:
    /// <c>2026-07-28</c>, <c>2025-11-25</c>, <c>2025-06-18</c>, <c>2025-03-26</c>, or <c>2024-11-05</c>.
    /// </exception>
    /// <remarks>This method is expected to throw exceptions when one or more conditions fails to be in a valid state.</remarks>
    public void ValidateOptions()
    {
        Validator.ThrowIfInvalidState(string.IsNullOrWhiteSpace(Version), $"{nameof(Version)} cannot be null, empty, or consist only of white-space characters.");
        Validator.ThrowIfInvalidState(!McpDocumentProtocolVersions.Supported.Contains(Version), $"{nameof(Version)} must be one of the supported MCP revisions: {string.Join(", ", McpDocumentProtocolVersions.Supported)}.");
    }
}
