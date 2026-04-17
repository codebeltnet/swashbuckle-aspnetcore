using Cuemon.Configuration;

namespace Codebelt.Extensions.Swashbuckle.AspNetCore.ModelContextProtocol
{
    /// <summary>
    /// Provides programmatic configuration for the <see cref="McpDocumentFilter"/> class.
    /// </summary>
    public class McpDocumentOptions : IParameterObject
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
        /// <item><term><see cref="EnableLegacySse"/></term><description><c>false</c></description></item>
        /// </list>
        /// </remarks>
        public McpDocumentOptions()
        {
            Pattern = "/mcp";
            TagName = "MCP";
            IncludeTools = true;
            EnableLegacySse = false;
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
        /// Gets or sets a value indicating whether the legacy SSE transport endpoints
        /// (<c>{Pattern}/sse</c> and <c>{Pattern}/message</c>) should also be documented.
        /// </summary>
        /// <value>Defaults to <c>false</c>.</value>
        public bool EnableLegacySse { get; set; }
    }
}
