using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Codebelt.Extensions.Xunit;
using Codebelt.Extensions.Xunit.Hosting.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using ModelContextProtocol.AspNetCore;
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
        public async Task AddMcpServer_ShouldUseConfiguredVersion()
        {
            using var filter = WebHostTestFactory.Create(services =>
            {
                services.AddRouting();
                services.AddEndpointsApiExplorer();
                services.AddSwaggerGen(o => o.AddMcpServer(o =>
                {
                    o.IncludeTools = false;
                    o.Version = "2025-03-26";
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

            Assert.Contains("2025-03-26", result);
            Assert.Contains("batch array", result);
        }

        [Fact]
        public async Task AddMcpServer_ShouldApplyConfigurationOnlyToTheNamedOpenApiDocument()
        {
            using var filter = WebHostTestFactory.Create(services =>
            {
                services.AddRouting();
                services.AddEndpointsApiExplorer();
                services.AddSwaggerGen(options =>
                {
                    options.SwaggerDoc("legacy-mcp", new OpenApiInfo { Title = "Legacy MCP", Version = "2025-11-25" });
                    options.SwaggerDoc("modern-mcp", new OpenApiInfo { Title = "Modern MCP", Version = "2026-07-28" });
                    options.SwaggerDoc("rest", new OpenApiInfo { Title = "REST API", Version = "v1" });

                    options.AddMcpServer("legacy-mcp", mcp =>
                    {
                        mcp.Version = "2025-11-25";
                        mcp.SessionMode = HttpServerSessionMode.Stateful;
                        mcp.IncludeTools = false;
                    });
                    options.AddMcpServer("modern-mcp", mcp =>
                    {
                        mcp.Version = "2026-07-28";
                        mcp.SessionMode = HttpServerSessionMode.Stateless;
                        mcp.IncludeTools = false;
                    });
                });
            }, app =>
            {
                app.UseRouting();
                app.UseSwagger();
                app.UseSwaggerUI();
            });

            var client = filter.Host.GetTestClient();
            using var legacyDocument = JsonDocument.Parse(await client.GetStringAsync("/swagger/legacy-mcp/swagger.json"));
            using var modernDocument = JsonDocument.Parse(await client.GetStringAsync("/swagger/modern-mcp/swagger.json"));
            using var restDocument = JsonDocument.Parse(await client.GetStringAsync("/swagger/rest/swagger.json"));

            var legacyPaths = legacyDocument.RootElement.GetProperty("paths");
            var modernPaths = modernDocument.RootElement.GetProperty("paths");
            var restPaths = restDocument.RootElement.GetProperty("paths");
            var legacyMcpPath = legacyPaths.GetProperty("/mcp");
            var modernMcpPath = modernPaths.GetProperty("/mcp");
            var legacyDescription = legacyMcpPath.GetProperty("post").GetProperty("description").GetString();
            var modernDescription = modernMcpPath.GetProperty("post").GetProperty("description").GetString();

            Assert.Contains("2025-11-25", legacyDescription);
            Assert.Contains("MCP-Protocol-Version", legacyDescription);
            Assert.True(legacyMcpPath.TryGetProperty("get", out _));
            Assert.True(legacyMcpPath.TryGetProperty("delete", out _));
            Assert.Contains("2026-07-28", modernDescription);
            Assert.Contains("clientCapabilities", modernDescription);
            Assert.False(modernMcpPath.TryGetProperty("get", out _));
            Assert.False(modernMcpPath.TryGetProperty("delete", out _));
            Assert.False(restPaths.TryGetProperty("/mcp", out _));
        }

        [Fact]
        public async Task AddMcpServer_ShouldDocumentUniqueDescriptionForEachVersion()
        {
            var testCases = new[]
            {
                (Version: "2026-07-28", SessionMode: HttpServerSessionMode.Stateless, EnableLegacySse: false, IncludeTools: false, Path: "/mcp", Method: "post"),
                (Version: "2025-11-25", SessionMode: HttpServerSessionMode.Stateful, EnableLegacySse: false, IncludeTools: false, Path: "/mcp", Method: "post"),
                (Version: "2025-06-18", SessionMode: HttpServerSessionMode.StatefulForInitializeClients, EnableLegacySse: true, IncludeTools: false, Path: "/mcp", Method: "post"),
                (Version: "2025-03-26", SessionMode: HttpServerSessionMode.Stateless, EnableLegacySse: false, IncludeTools: false, Path: "/mcp", Method: "post"),
                (Version: "2024-11-05", SessionMode: HttpServerSessionMode.Stateful, EnableLegacySse: false, IncludeTools: false, Path: "/mcp/sse", Method: "get")
            };
            var descriptions = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (var testCase in testCases)
            {
                using var filter = WebHostTestFactory.Create(services =>
                {
                    services.AddRouting();
                    services.AddEndpointsApiExplorer();
                    services.AddSwaggerGen(o => o.AddMcpServer(mcp =>
                    {
                        mcp.Version = testCase.Version;
                        mcp.SessionMode = testCase.SessionMode;
                        mcp.EnableLegacySse = testCase.EnableLegacySse;
                        mcp.IncludeTools = testCase.IncludeTools;
                    }));
                }, app =>
                {
                    app.UseRouting();
                    app.UseSwagger();
                    app.UseSwaggerUI();
                });

                var client = filter.Host.GetTestClient();
                var result = await client.GetStringAsync("/swagger/v1/swagger.json");
                using var document = JsonDocument.Parse(result);
                var description = document.RootElement
                    .GetProperty("paths")
                    .GetProperty(testCase.Path)
                    .GetProperty(testCase.Method)
                    .GetProperty("description")
                    .GetString();

                Assert.False(string.IsNullOrWhiteSpace(description));
                Assert.Contains(testCase.Version, description);
                Assert.True(descriptions.TryAdd(testCase.Version, description), $"Duplicate version: {testCase.Version}");
                TestOutput.WriteLine($"{testCase.Version}: {description}");
            }

            var uniqueDescriptions = new HashSet<string>(descriptions.Values, StringComparer.Ordinal);
            Assert.Equal(testCases.Length, uniqueDescriptions.Count);
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
                services.AddSwaggerGen(o => o.AddMcpServer(o =>
                {
                    o.EnableLegacySse = true;
                    o.SessionMode = HttpServerSessionMode.Stateful;
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
                    o.SessionMode = HttpServerSessionMode.Stateful;
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
                services.AddSwaggerGen(o => o.AddMcpServer(o =>
                {
                    o.EnableLegacySse = true;
                    o.SessionMode = HttpServerSessionMode.Stateful;
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

            Assert.Contains("\"operationId\": \"mcp-sse\"", result);
            Assert.Contains("\"operationId\": \"mcp-sse-message\"", result);
        }
    }
}
