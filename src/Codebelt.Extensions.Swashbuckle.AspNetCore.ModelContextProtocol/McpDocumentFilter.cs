using Swashbuckle.AspNetCore.SwaggerGen;
using Codebelt.Extensions.Swashbuckle.AspNetCore.ModelContextProtocol.Versions;
using Microsoft.OpenApi;

namespace Codebelt.Extensions.Swashbuckle.AspNetCore.ModelContextProtocol;

/// <summary>
/// A Swashbuckle <see cref="IDocumentFilter"/> that injects the configured MCP transport
/// into the generated OpenAPI document.
/// </summary>
/// <seealso cref="DocumentFilter{T}"/>
/// <seealso cref="McpDocumentOptions"/>
public class McpDocumentFilter : DocumentFilter<McpDocumentOptions>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="McpDocumentFilter"/> class.
    /// </summary>
    /// <param name="options">The options that control which document and routes are documented.</param>
    public McpDocumentFilter(McpDocumentOptions options) : base(options)
    {
    }

    /// <summary>
    /// Applies post-processing to the <paramref name="swaggerDoc"/>.
    /// </summary>
    /// <param name="swaggerDoc">The <see cref="OpenApiDocument"/> to modify.</param>
    /// <param name="context">The <see cref="DocumentFilterContext"/> that provides additional context.</param>
    /// <remarks>
    /// The selected protocol revision is read from <see cref="McpDocumentOptions.Version"/>.
    /// </remarks>
    public override void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        McpDocumentFilterFactory.Create(Options).Apply(swaggerDoc, context);
    }
}
