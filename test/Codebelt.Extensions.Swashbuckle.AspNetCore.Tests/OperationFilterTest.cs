using System;
using Codebelt.Extensions.Xunit;
using Cuemon.Configuration;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Xunit;

namespace Codebelt.Extensions.Swashbuckle.AspNetCore
{
    public class OperationFilterTest : Test
    {
        public OperationFilterTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void OperationFilter_ShouldInvokeDerivedImplementation()
        {
            var operation = new OpenApiOperation();
            var wasCalled = false;
            var sut = new CallbackOperationFilter((actualOperation, actualContext) =>
            {
                wasCalled = true;
                Assert.Same(operation, actualOperation);
                Assert.Null(actualContext);
            });

            sut.Apply(operation, null);

            Assert.True(wasCalled);
        }

        [Fact]
        public void OperationFilter_Generic_ShouldExposeConfiguredOptions()
        {
            var options = new FakeOperationFilterOptions();

            var sut = new ConfigurableOperationFilter(options);

            Assert.Same(options, sut.Options);
        }

        [Fact]
        public void OperationFilter_Generic_ShouldThrowArgumentNullException_WhenOptionsIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ConfigurableOperationFilter(null));
        }

        private sealed class CallbackOperationFilter : OperationFilter
        {
            private readonly Action<OpenApiOperation, OperationFilterContext> _callback;

            public CallbackOperationFilter(Action<OpenApiOperation, OperationFilterContext> callback)
            {
                _callback = callback;
            }

            public override void Apply(OpenApiOperation operation, OperationFilterContext context)
            {
                _callback(operation, context);
            }
        }

        private sealed class ConfigurableOperationFilter : OperationFilter<FakeOperationFilterOptions>
        {
            public ConfigurableOperationFilter(FakeOperationFilterOptions options) : base(options)
            {
            }

            public override void Apply(OpenApiOperation operation, OperationFilterContext context)
            {
            }
        }

        private sealed class FakeOperationFilterOptions : IParameterObject
        {
        }
    }
}
