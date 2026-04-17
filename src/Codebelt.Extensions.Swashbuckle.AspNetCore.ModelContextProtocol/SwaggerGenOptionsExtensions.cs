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
            Validator.ThrowIfInvalidConfigurator(setup, out var mcpOptions);
            options.AddDocumentFilterInstance(new McpDocumentFilter(mcpOptions));
        }
    }
}
