using Asp.Versioning;
using Codebelt.Extensions.Asp.Versioning;
using Codebelt.Extensions.Swashbuckle.AspNetCore.Assets.V1;
using Codebelt.Extensions.Xunit;
using Codebelt.Extensions.Xunit.Hosting.AspNetCore;
using Cuemon.Extensions.AspNetCore.Mvc.Formatters.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Codebelt.Extensions.Swashbuckle.AspNetCore.Assets;
using Codebelt.SharedKernel;
using Cuemon.AspNetCore.Diagnostics;
using Cuemon.Diagnostics;
using Cuemon.Extensions.AspNetCore.Diagnostics;
using Xunit;

namespace Codebelt.Extensions.Swashbuckle.AspNetCore
{
    public class ServiceCollectionExtensionsTest : Test
    {
        public ServiceCollectionExtensionsTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task AddRestfulSwagger_ShouldLoadXmlDocumentationFiles_UsingAddFromBaseDirectory_WithControllers()
        {
            using (var filter = WebHostTestFactory.Create(services =>
                   {
                       services.AddControllers().AddApplicationPart(typeof(FakeController).Assembly)
                           .AddJsonFormatters();
                       services.AddEndpointsApiExplorer();
                        services.AddRestfulApiVersioning(o =>
                       {
                           o.Conventions.Controller<FakeController>().HasApiVersion(new ApiVersion(1, 0));
                       });
                       services.AddRestfulSwagger(o =>
                       {
                           o.Settings.UseAllOfToExtendReferenceSchemas();
                           o.XmlDocumentations.AddFromBaseDirectory(typeof(FakeModel));
                           o.XmlDocumentations.AddFromBaseDirectory(typeof(Token));
                           o.XmlDocumentations.AddFromReferencePacks(typeof(ProblemDetails));
                       });
                       services.AddFaultDescriptorOptions(o =>
                           o.FaultDescriptor = PreferredFaultDescriptor.ProblemDetails);
                       services.PostConfigureAllExceptionDescriptorOptions(o =>
                           o.SensitivityDetails = FaultSensitivityDetails.All);
                   }, app =>
                   {
                       app.UseFaultDescriptorExceptionHandler();
                       app.UseRouting();
                       app.UseEndpoints(routes => { routes.MapControllers(); });
                       app.UseSwagger();
                       app.UseSwaggerUI();
                       app.UseRestfulApiVersioning();
                   }))
            {
                var options = filter.Host.Services.GetRequiredService<IOptions<RestfulSwaggerOptions>>().Value;

                TestOutput.WriteLine($"XmlDocumentations count: {options.XmlDocumentations.Count}");

                Assert.NotEmpty(options.XmlDocumentations);

                var client = filter.Host.GetTestClient();
                var result = await client.GetStringAsync("/swagger/v1/swagger.json");

                TestOutput.WriteLine(result);

                Assert.Contains("\"summary\":", result);
                Assert.Contains("Gets an OK response with a body of Functional Test V1.", result);
                Assert.Contains("FakeModel", result);
                Assert.Contains("A fake model for functional testing purposes.", result);
                Assert.Contains("Gets or sets the message.", result);
                Assert.Contains("Token", result);
                Assert.Contains("an immutable value used for identification or access control", result);
                Assert.Contains("Gets an OK response with a body of Token.", result);
                Assert.Contains("Gets the value of this instance.", result);
                Assert.Contains("ProblemDetails", result);
                Assert.Contains("A machine-readable format for specifying errors in HTTP API responses based on", result);
                Assert.Contains("A URI reference [RFC3986] that identifies the problem type.", result);
                Assert.Contains("A short, human-readable summary of the problem type.", result);
                Assert.Contains("A human-readable explanation specific to this occurrence of the problem.", result);
            }
        }

        [Fact]
        public async Task AddRestfulSwagger_ShouldLoadXmlDocumentationFiles_UsingAddFromBaseDirectory_WithMinimalApi()
        {
            using (var filter = MinimalWebHostTestFactory.Create(services =>
                   {
                       services.AddEndpointsApiExplorer();
                       services.AddRestfulApiVersioning();
                       services.AddRestfulSwagger(o =>
                       {
                           o.Settings.UseAllOfToExtendReferenceSchemas();
                           o.XmlDocumentations.AddFromBaseDirectory<FakeModel>();
                           o.XmlDocumentations.AddFromBaseDirectory<Token>();
                           o.XmlDocumentations.AddFromReferencePacks<ProblemDetails>();
                       });
                       services.AddFaultDescriptorOptions(o =>
                           o.FaultDescriptor = PreferredFaultDescriptor.ProblemDetails);
                       services.PostConfigureAllExceptionDescriptorOptions(o =>
                           o.SensitivityDetails = FaultSensitivityDetails.All);
                   }, app =>
                   {
                       app.UseFaultDescriptorExceptionHandler();
                       app.UseRouting();
                       app.UseEndpoints(endpoints =>
                       {
                           var fake = endpoints.NewVersionedApi("Fake");
                           var fakeV1 = fake.MapGroup("/fake")
                               .HasApiVersion(new ApiVersion(1, 0));
                           fakeV1.MapGet("/", () => Results.Ok(new FakeModel { Message = "Functional Test V1" }))
                               .WithSummary("Gets an OK response with a body of Functional Test V1.")
                               .Produces<FakeModel>()
                               .ProducesProblem(StatusCodes.Status400BadRequest);
                           fakeV1.MapGet("/token", () => Results.Ok(new Token("Functional Test V1")))
                               .WithSummary("Gets an OK response with a body of Token.")
                               .Produces<Token>()
                               .ProducesProblem(StatusCodes.Status400BadRequest);
                       });
                       app.UseSwagger();
                       app.UseSwaggerUI();
                       app.UseRestfulApiVersioning();
                   }))
            {
                var options = filter.Host.Services.GetRequiredService<IOptions<RestfulSwaggerOptions>>().Value;

                TestOutput.WriteLine($"XmlDocumentations count: {options.XmlDocumentations.Count}");

                Assert.NotEmpty(options.XmlDocumentations);

                var client = filter.Host.GetTestClient();
                var result = await client.GetStringAsync("/swagger/v1/swagger.json");

                TestOutput.WriteLine(result);

                Assert.Contains("\"summary\":", result);
                Assert.Contains("Gets an OK response with a body of Functional Test V1.", result);
                Assert.Contains("FakeModel", result);
                Assert.Contains("A fake model for functional testing purposes.", result);
                Assert.Contains("Gets or sets the message.", result);
                Assert.Contains("Token", result);
                Assert.Contains("an immutable value used for identification or access control", result);
                Assert.Contains("Gets an OK response with a body of Token.", result);
                Assert.Contains("Gets the value of this instance.", result);
                Assert.Contains("ProblemDetails", result);
                Assert.Contains("A machine-readable format for specifying errors in HTTP API responses based on", result);
                Assert.Contains("A URI reference [RFC3986] that identifies the problem type.", result);
                Assert.Contains("A short, human-readable summary of the problem type.", result);
                Assert.Contains("A human-readable explanation specific to this occurrence of the problem.", result);
            }
        }
    }
}
