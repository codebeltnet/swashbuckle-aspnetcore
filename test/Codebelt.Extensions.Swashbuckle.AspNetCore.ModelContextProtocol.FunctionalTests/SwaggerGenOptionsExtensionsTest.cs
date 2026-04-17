using System.Threading.Tasks;
using Codebelt.Extensions.Xunit;
using Codebelt.Extensions.Xunit.Hosting.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Codebelt.Extensions.Swashbuckle.AspNetCore.ModelContextProtocol
{
    /// <summary>
    /// Functional tests for the <see cref="SwaggerGenOptionsExtensions"/> class.
    /// </summary>
    public class SwaggerGenOptionsExtensionsTest : Test
    {
        public SwaggerGenOptionsExtensionsTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task AddMcpServer_ShouldAddMcpPathToOpenApiDocument_WithDefaultOptions()
        {
            using var filter = WebHostTestFactory.Create(services =>
            {
                services.AddRouting();
                services.AddEndpointsApiExplorer();
                services.AddSwaggerGen(o => o.AddMcpServer());
            }, app =>
            {
                app.UseRouting();
                app.UseSwagger();
                app.UseSwaggerUI();
            });

            var client = filter.Host.GetTestClient();
            var result = await client.GetStringAsync("/swagger/v1/swagger.json");

            TestOutput.WriteLine(result);

            Assert.Contains("\"/mcp\"", result);
        }

        [Fact]
        public async Task AddMcpServer_ShouldAddMcpTagToOpenApiDocument_WithDefaultOptions()
        {
            using var filter = WebHostTestFactory.Create(services =>
            {
                services.AddRouting();
                services.AddEndpointsApiExplorer();
                services.AddSwaggerGen(o => o.AddMcpServer());
            }, app =>
            {
                app.UseRouting();
                app.UseSwagger();
                app.UseSwaggerUI();
            });

            var client = filter.Host.GetTestClient();
            var result = await client.GetStringAsync("/swagger/v1/swagger.json");

            TestOutput.WriteLine(result);

            Assert.Contains("\"MCP\"", result);
            Assert.Contains("Model Context Protocol", result);
        }

        [Fact]
        public async Task AddMcpServer_ShouldNotAddLegacySsePaths_WhenDisabledByDefault()
        {
            using var filter = WebHostTestFactory.Create(services =>
            {
                services.AddRouting();
                services.AddEndpointsApiExplorer();
                services.AddSwaggerGen(o => o.AddMcpServer());
            }, app =>
            {
                app.UseRouting();
                app.UseSwagger();
                app.UseSwaggerUI();
            });

            var client = filter.Host.GetTestClient();
            var result = await client.GetStringAsync("/swagger/v1/swagger.json");

            TestOutput.WriteLine(result);

            Assert.DoesNotContain("\"/mcp/sse\"", result);
            Assert.DoesNotContain("\"/mcp/message\"", result);
        }

        [Fact]
        public async Task AddMcpServer_ShouldAddLegacySsePaths_WhenEnabled()
        {
            using var filter = WebHostTestFactory.Create(services =>
            {
                services.AddRouting();
                services.AddEndpointsApiExplorer();
                services.AddSwaggerGen(o => o.AddMcpServer(o => o.EnableLegacySse = true));
            }, app =>
            {
                app.UseRouting();
                app.UseSwagger();
                app.UseSwaggerUI();
            });

            var client = filter.Host.GetTestClient();
            var result = await client.GetStringAsync("/swagger/v1/swagger.json");

            TestOutput.WriteLine(result);

            Assert.Contains("\"/mcp/sse\"", result);
            Assert.Contains("\"/mcp/message\"", result);
        }

        [Fact]
        public async Task AddMcpServer_ShouldUseCustomPattern_WhenPatternIsOverridden()
        {
            using var filter = WebHostTestFactory.Create(services =>
            {
                services.AddRouting();
                services.AddEndpointsApiExplorer();
                services.AddSwaggerGen(o => o.AddMcpServer(o =>
                {
                    o.Pattern = "/ai/mcp";
                    o.EnableLegacySse = true;
                }));
            }, app =>
            {
                app.UseRouting();
                app.UseSwagger();
                app.UseSwaggerUI();
            });

            var client = filter.Host.GetTestClient();
            var result = await client.GetStringAsync("/swagger/v1/swagger.json");

            TestOutput.WriteLine(result);

            Assert.Contains("\"/ai/mcp\"", result);
            Assert.Contains("\"/ai/mcp/sse\"", result);
            Assert.Contains("\"/ai/mcp/message\"", result);
            Assert.DoesNotContain("\"/mcp\"", result);
        }

        [Fact]
        public async Task AddMcpServer_ShouldIncludeJsonRpc2RequestBodySchema_ForStreamableHttpPath()
        {
            using var filter = WebHostTestFactory.Create(services =>
            {
                services.AddRouting();
                services.AddEndpointsApiExplorer();
                services.AddSwaggerGen(o => o.AddMcpServer());
            }, app =>
            {
                app.UseRouting();
                app.UseSwagger();
                app.UseSwaggerUI();
            });

            var client = filter.Host.GetTestClient();
            var result = await client.GetStringAsync("/swagger/v1/swagger.json");

            TestOutput.WriteLine(result);

            Assert.Contains("\"jsonrpc\"", result);
            Assert.Contains("JSON-RPC 2.0", result);
            Assert.Contains("application/json", result);
        }

        [Fact]
        public async Task AddMcpServer_ShouldHaveOperationIdMcp_ForStreamableHttpPath()
        {
            using var filter = WebHostTestFactory.Create(services =>
            {
                services.AddRouting();
                services.AddEndpointsApiExplorer();
                services.AddSwaggerGen(o => o.AddMcpServer());
            }, app =>
            {
                app.UseRouting();
                app.UseSwagger();
                app.UseSwaggerUI();
            });

            var client = filter.Host.GetTestClient();
            var result = await client.GetStringAsync("/swagger/v1/swagger.json");

            TestOutput.WriteLine(result);

            Assert.Contains("\"operationId\": \"mcp\"", result);
        }

        [Fact]
        public async Task AddMcpServer_ShouldHaveOperationIdMcpSse_ForLegacySsePath()
        {
            using var filter = WebHostTestFactory.Create(services =>
            {
                services.AddRouting();
                services.AddEndpointsApiExplorer();
                services.AddSwaggerGen(o => o.AddMcpServer(o => o.EnableLegacySse = true));
            }, app =>
            {
                app.UseRouting();
                app.UseSwagger();
                app.UseSwaggerUI();
            });

            var client = filter.Host.GetTestClient();
            var result = await client.GetStringAsync("/swagger/v1/swagger.json");

            TestOutput.WriteLine(result);

            Assert.Contains("\"operationId\": \"mcp-sse\"", result);
            Assert.Contains("\"operationId\": \"mcp-sse-message\"", result);
        }
    }
}
