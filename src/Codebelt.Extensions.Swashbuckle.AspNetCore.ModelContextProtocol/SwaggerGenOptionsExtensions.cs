using System;
using Cuemon;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Codebelt.Extensions.Swashbuckle.AspNetCore.ModelContextProtocol
{
    /// <summary>
    /// Extension methods for the <see cref="SwaggerGenOptions"/> class.
    /// </summary>
    public static class SwaggerGenOptionsExtensions
    {
        /// <summary>
        /// Adds a <see cref="McpDocumentFilter"/> to the <see cref="SwaggerGenOptions.DocumentFilterDescriptors"/> so that the MCP endpoint(s) appear in the generated OpenAPI document.
        /// </summary>
        /// <param name="options">The <see cref="SwaggerGenOptions"/> to extend.</param>
        /// <param name="setup">The <see cref="McpDocumentOptions"/> that may be configured.</param>
        public static void AddMcpServer(this SwaggerGenOptions options, Action<McpDocumentOptions> setup = null)
        {

            Validator.ThrowIfNull(options);
            Validator.ThrowIfInvalidConfigurator(setup, out var mcpOptions);
            options.AddDocumentFilterInstance(new McpDocumentFilter(mcpOptions));
        }

        /// <summary>
        /// Adds a <see cref="McpDocumentFilter"/> that applies only to the named OpenAPI document.
        /// </summary>
        /// <param name="options">The <see cref="SwaggerGenOptions"/> to extend.</param>
        /// <param name="documentName">The name passed to <c>SwaggerDoc</c> for the OpenAPI document that should include the MCP endpoints.</param>
        /// <param name="setup">The <see cref="McpDocumentOptions"/> that may be configured.</param>
        /// <exception cref="ArgumentNullException"><paramref name="options"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="documentName"/> is <c>null</c>, empty, or consists only of white-space characters.</exception>
        /// <remarks>
        /// Call this overload once for each document that needs a distinct MCP version or transport configuration.
        /// The existing overload without <paramref name="documentName"/> continues to apply its configuration to every document.
        /// </remarks>
        public static void AddMcpServer(this SwaggerGenOptions options, string documentName, Action<McpDocumentOptions> setup)
        {
            Validator.ThrowIfNull(options);
            Validator.ThrowIfNullOrWhitespace(documentName);
            Validator.ThrowIfInvalidConfigurator(setup, out var mcpOptions);
            options.AddDocumentFilterInstance(new DocumentScopedMcpDocumentFilter(documentName, mcpOptions));
        }

        private sealed class DocumentScopedMcpDocumentFilter : McpDocumentFilter
        {
            private readonly string _documentName;

            public DocumentScopedMcpDocumentFilter(string documentName, McpDocumentOptions options) : base(options)
            {
                _documentName = documentName;
            }

            public override void Apply(Microsoft.OpenApi.OpenApiDocument swaggerDoc, DocumentFilterContext context)
            {
                if (string.Equals(context.DocumentName, _documentName, StringComparison.Ordinal))
                {
                    base.Apply(swaggerDoc, context);
                }
            }
        }
    }
}
