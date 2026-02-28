using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Xml.XPath;
using Codebelt.Extensions.Xunit;
using Xunit;

namespace Codebelt.Extensions.Swashbuckle.AspNetCore
{
    /// <summary>
    /// Tests for the <see cref="XPathDocumentExtensions"/> class.
    /// </summary>
    public class XPathDocumentExtensionsTest : Test
    {
        public XPathDocumentExtensionsTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void AddByType_ShouldThrowArgumentNullException_WhenTypeIsNull()
        {
            var documents = new List<XPathDocument>();

            Assert.Throws<ArgumentNullException>(() => documents.AddByType(null));
        }

        [Fact]
        public void AddByType_ShouldAddDocument_WhenXmlFileExistsForType()
        {
            var documents = new List<XPathDocument>();

            var result = documents.AddByType(typeof(XPathDocumentExtensionsTest));

            Assert.Same(documents, result);
            Assert.NotEmpty(documents);
        }

        [Fact]
        public void AddByType_ShouldReturnSameListReference()
        {
            var documents = new List<XPathDocument>();

            var result = documents.AddByType(typeof(XPathDocumentExtensionsTest));

            Assert.Same(documents, result);
        }

        [Fact]
        public void AddByAssembly_ShouldThrowArgumentNullException_WhenAssemblyIsNull()
        {
            var documents = new List<XPathDocument>();

            Assert.Throws<ArgumentNullException>(() => documents.AddByAssembly(null));
        }

        [Fact]
        public void AddByAssembly_ShouldAddDocument_WhenXmlFileExistsForAssembly()
        {
            var documents = new List<XPathDocument>();

            var result = documents.AddByAssembly(typeof(XPathDocumentExtensionsTest).Assembly);

            Assert.Same(documents, result);
            Assert.NotEmpty(documents);
        }

        [Fact]
        public void AddByAssembly_ShouldNotAddDocument_WhenAssemblyIsDynamic()
        {
            var documents = new List<XPathDocument>();
            var dynamicAssembly = System.Reflection.Emit.AssemblyBuilder.DefineDynamicAssembly(
                new System.Reflection.AssemblyName("DynamicTestAssembly"),
                System.Reflection.Emit.AssemblyBuilderAccess.Run);

            var result = documents.AddByAssembly(dynamicAssembly);

            Assert.Same(documents, result);
            Assert.Empty(documents);
        }

        [Fact]
        public void AddByFilename_ShouldThrowArgumentNullException_WhenDocumentsIsNull()
        {
            IList<XPathDocument> documents = null;

            Assert.Throws<ArgumentNullException>(() => documents.AddByFilename("some-path.xml"));
        }

        [Fact]
        public void AddByFilename_ShouldThrowArgumentNullException_WhenPathIsNull()
        {
            var documents = new List<XPathDocument>();

            Assert.Throws<ArgumentNullException>(() => documents.AddByFilename(null));
        }

        [Fact]
        public void AddByFilename_ShouldThrowArgumentException_WhenPathIsWhitespace()
        {
            var documents = new List<XPathDocument>();

            Assert.Throws<ArgumentException>(() => documents.AddByFilename("   "));
        }

        [Fact]
        public void AddByFilename_ShouldNotAddDocument_WhenFileDoesNotExist()
        {
            var documents = new List<XPathDocument>();

            var result = documents.AddByFilename(Path.Combine(Path.GetTempPath(), "non-existing-file-" + Guid.NewGuid() + ".xml"));

            Assert.Same(documents, result);
            Assert.Empty(documents);
        }

        [Fact]
        public void AddByFilename_ShouldAddDocument_WhenFileExists()
        {
            var documents = new List<XPathDocument>();
            var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".xml");
            File.WriteAllText(tempFile, "<?xml version=\"1.0\"?><doc><members></members></doc>");
            try
            {
                var result = documents.AddByFilename(tempFile);

                Assert.Same(documents, result);
                Assert.Single(documents);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public void AddByFilename_ShouldAddMultipleDocuments_WhenCalledMultipleTimes()
        {
            var documents = new List<XPathDocument>();
            var tempFile1 = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".xml");
            var tempFile2 = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".xml");
            File.WriteAllText(tempFile1, "<?xml version=\"1.0\"?><doc><members></members></doc>");
            File.WriteAllText(tempFile2, "<?xml version=\"1.0\"?><doc><members></members></doc>");
            try
            {
                documents.AddByFilename(tempFile1).AddByFilename(tempFile2);

                Assert.Equal(2, documents.Count);
            }
            finally
            {
                File.Delete(tempFile1);
                File.Delete(tempFile2);
            }
        }

        [Fact]
        public void AddFromBaseDirectory_ShouldThrowArgumentNullException_WhenDocumentsIsNull()
        {
            IList<XPathDocument> documents = null;

            Assert.Throws<ArgumentNullException>(() => documents.AddFromBaseDirectory(typeof(XPathDocumentExtensionsTest)));
        }

        [Fact]
        public void AddFromBaseDirectory_ShouldReturnSameListReference()
        {
            var documents = new List<XPathDocument>();

            var result = documents.AddFromBaseDirectory(typeof(XPathDocumentExtensionsTest));

            Assert.Same(documents, result);
        }

        [Fact]
        public void AddFromBaseDirectory_ShouldAddDocument_WhenMatchingXmlFileExistsInBaseDirectory()
        {
            var documents = new List<XPathDocument>();

            var result = documents.AddFromBaseDirectory(typeof(XPathDocumentExtensionsTest));

            Assert.Same(documents, result);
            Assert.NotEmpty(documents);
        }

        [Fact]
        public void AddByAssemblyContext_ShouldThrowArgumentNullException_WhenDocumentsIsNull()
        {
            IList<XPathDocument> documents = null;

            Assert.Throws<ArgumentNullException>(() => documents.AddByAssemblyContext());
        }

        [Fact]
        public void AddByAssemblyContext_ShouldReturnSameListReference()
        {
            var documents = new List<XPathDocument>();

            var result = documents.AddByAssemblyContext();

            Assert.Same(documents, result);
        }

        [Fact]
        public void AddByAssemblyContext_ShouldAddDocuments_WhenMatchingXmlFilesExistInBaseDirectory()
        {
            var documents = new List<XPathDocument>();

            var result = documents.AddByAssemblyContext();

            Assert.Same(documents, result);
            Assert.NotEmpty(documents);
        }

        [Fact]
        public void AddByFilename_ShouldSupportFluentChaining()
        {
            var documents = new List<XPathDocument>();
            var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".xml");
            File.WriteAllText(tempFile, "<?xml version=\"1.0\"?><doc><members></members></doc>");
            try
            {
                var result = documents
                    .AddByFilename(tempFile)
                    .AddByFilename(Path.Combine(Path.GetTempPath(), "does-not-exist.xml"));

                Assert.Same(documents, result);
                Assert.Single(documents);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public void AddByType_ShouldNotAddDuplicateDocuments_WhenCalledMultipleTimes()
        {
            var documents = new List<XPathDocument>();

            documents.AddByType(typeof(XPathDocumentExtensionsTest));
            documents.AddByType(typeof(XPathDocumentExtensionsTest));

            Assert.Equal(2, documents.Count);
        }

        [Fact]
        public void AddFromReferencePacks_ShouldThrowArgumentNullException_WhenDocumentsIsNull()
        {
            IList<XPathDocument> documents = null;

            Assert.Throws<ArgumentNullException>(() => documents.AddFromReferencePacks(typeof(object)));
        }

        [Fact]
        public void AddFromReferencePacks_ShouldThrowArgumentNullException_WhenTypeIsNull()
        {
            var documents = new List<XPathDocument>();

            Assert.Throws<ArgumentNullException>(() => documents.AddFromReferencePacks(null));
        }

        [Fact]
        public void AddFromReferencePacks_ShouldReturnSameListReference()
        {
            var documents = new List<XPathDocument>();

            var result = documents.AddFromReferencePacks(typeof(object));

            Assert.Same(documents, result);
        }

        [Fact]
        public void AddFromReferencePacks_ShouldNotAddDocument_WhenAssemblyIsDynamic()
        {
            var documents = new List<XPathDocument>();
            var dynamicAssembly = System.Reflection.Emit.AssemblyBuilder.DefineDynamicAssembly(
                new System.Reflection.AssemblyName("DynamicRefPackAssembly"),
                System.Reflection.Emit.AssemblyBuilderAccess.Run);
            var dynamicType = dynamicAssembly.DefineDynamicModule("Main").DefineType("FakeType").CreateType();

            var result = documents.AddFromReferencePacks(dynamicType);

            Assert.Same(documents, result);
            Assert.Empty(documents);
        }

        [Fact]
        public void AddFromReferencePacks_ShouldAddDocument_WhenRefPackXmlExistsForType()
        {
            var documents = new List<XPathDocument>();

            var result = documents.AddFromReferencePacks(typeof(JsonSerializer));

            Assert.Same(documents, result);
            Assert.NotEmpty(documents);
        }

        [Fact]
        public void AddFromNuGetPackages_ShouldThrowArgumentNullException_WhenDocumentsIsNull()
        {
            IList<XPathDocument> documents = null;

            Assert.Throws<ArgumentNullException>(() => documents.AddFromNuGetPackages(typeof(object)));
        }

        [Fact]
        public void AddFromNuGetPackages_ShouldThrowArgumentNullException_WhenTypeIsNull()
        {
            var documents = new List<XPathDocument>();

            Assert.Throws<ArgumentNullException>(() => documents.AddFromNuGetPackages(null));
        }

        [Fact]
        public void AddFromNuGetPackages_ShouldReturnSameListReference()
        {
            var documents = new List<XPathDocument>();

            var result = documents.AddFromNuGetPackages(typeof(global::Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions));

            Assert.Same(documents, result);
        }

        [Fact]
        public void AddFromNuGetPackages_ShouldNotAddDocument_WhenAssemblyIsDynamic()
        {
            var documents = new List<XPathDocument>();
            var dynamicAssembly = System.Reflection.Emit.AssemblyBuilder.DefineDynamicAssembly(
                new System.Reflection.AssemblyName("DynamicNuGetAssembly"),
                System.Reflection.Emit.AssemblyBuilderAccess.Run);
            var dynamicType = dynamicAssembly.DefineDynamicModule("Main").DefineType("FakeType").CreateType();

            var result = documents.AddFromNuGetPackages(dynamicType);

            Assert.Same(documents, result);
            Assert.Empty(documents);
        }

        [Fact]
        public void AddFromNuGetPackages_ShouldAddDocument_WhenNuGetPackageXmlExistsForType()
        {
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var nugetCachePath = Path.Combine(userProfile, ".nuget", "packages");
            Assert.SkipWhen(!Directory.Exists(nugetCachePath), "NuGet global packages cache is not available in this environment.");

            var documents = new List<XPathDocument>();

            var result = documents.AddFromNuGetPackages(typeof(global::Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions));

            Assert.Same(documents, result);
            Assert.NotEmpty(documents);
        }

        [Fact]
        public void AddFromNuGetPackages_ShouldAddDocumentsForTypeHierarchy_WhenBaseTypeIsInDifferentPackage()
        {
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var nugetCachePath = Path.Combine(userProfile, ".nuget", "packages");
            Assert.SkipWhen(!Directory.Exists(nugetCachePath), "NuGet global packages cache is not available in this environment.");

            var documents = new List<XPathDocument>();

            var result = documents.AddFromNuGetPackages(typeof(global::Codebelt.SharedKernel.Token));

            Assert.Same(documents, result);
            Assert.True(documents.Count >= 2, $"Expected at least 2 documents (Token and its base type), but got {documents.Count}.");
        }
    }
}
