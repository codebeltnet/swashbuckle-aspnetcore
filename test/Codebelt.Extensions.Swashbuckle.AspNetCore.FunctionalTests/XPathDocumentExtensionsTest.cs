using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml.XPath;
using Codebelt.Extensions.Xunit;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Codebelt.Extensions.Swashbuckle.AspNetCore
{
    /// <summary>
    /// Functional tests for the <see cref="XPathDocumentExtensions"/> class, verifying that
    /// reference pack discovery works correctly across Linux distribution variants, including
    /// Alpine MCR (packs at <c>/usr/share/dotnet/packs</c>) and Docker Hardened Images
    /// (DHI, <c>dhi.io/aspnetcore</c>) where no packs directory is present and
    /// <see cref="XPathDocumentExtensions.AddFromReferencePacks{T}"/> gracefully returns
    /// an empty list.
    /// </summary>
    public class XPathDocumentExtensionsTest : Test
    {
        public XPathDocumentExtensionsTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void AddFromReferencePacks_ShouldDiscoverRefPacksAndLoadDocuments_OnCurrentPlatform()
        {
            LogEnvironmentInfo();

            var documents = new List<XPathDocument>();

            var result = documents.AddFromReferencePacks(typeof(ProblemDetails));

            Assert.Same(documents, result);

            if (ArePacksAvailable())
            {
                Assert.NotEmpty(documents);
                TestOutput.WriteLine($"SDK environment: {documents.Count} reference pack document(s) loaded.");
            }
            else
            {
                Assert.Empty(documents);
                TestOutput.WriteLine("Runtime-only environment (DHI): no reference packs present — graceful no-op confirmed.");
            }
        }

        [Fact]
        public void AddFromReferencePacks_ShouldContainProblemDetailsDocumentation_WhenAspNetCoreRefPackIsLoaded()
        {
            LogEnvironmentInfo();

            if (!ArePacksAvailable())
            {
                Assert.Skip("No reference packs present in this environment (DHI runtime-only image has no packs directory).");
            }

            var documents = new List<XPathDocument>();

            documents.AddFromReferencePacks(typeof(ProblemDetails));

            Assert.NotEmpty(documents);

            var found = false;
            foreach (var doc in documents)
            {
                var nav = doc.CreateNavigator();
                if (nav.SelectSingleNode("//member[@name='T:Microsoft.AspNetCore.Mvc.ProblemDetails']") != null)
                {
                    found = true;
                    break;
                }
            }

            Assert.True(found, "Expected ProblemDetails type documentation to be present in the loaded reference pack documents.");
        }

        [Fact]
        public void AddFromReferencePacks_Generic_ShouldDiscoverRefPacksAndLoadDocuments_OnCurrentPlatform()
        {
            LogEnvironmentInfo();

            var documents = new List<XPathDocument>();

            var result = documents.AddFromReferencePacks<ProblemDetails>();

            Assert.Same(documents, result);

            if (ArePacksAvailable())
            {
                Assert.NotEmpty(documents);
                TestOutput.WriteLine($"SDK environment: {documents.Count} reference pack document(s) loaded.");
            }
            else
            {
                Assert.Empty(documents);
                TestOutput.WriteLine("Runtime-only environment (DHI): no reference packs present — graceful no-op confirmed.");
            }
        }

        private void LogEnvironmentInfo()
        {
            TestOutput.WriteLine($"OS: {RuntimeInformation.OSDescription}");
            TestOutput.WriteLine($"RID: {RuntimeInformation.RuntimeIdentifier}");
            TestOutput.WriteLine($"DOTNET_ROOT: {Environment.GetEnvironmentVariable("DOTNET_ROOT") ?? "(not set)"}");
            TestOutput.WriteLine($"typeof(object).Assembly.Location: {typeof(object).Assembly.Location}");
        }

        private static bool ArePacksAvailable()
        {
            var dotnetRoot = Environment.GetEnvironmentVariable("DOTNET_ROOT");
            if (!string.IsNullOrWhiteSpace(dotnetRoot) && Directory.Exists(Path.Combine(dotnetRoot, "packs")))
            {
                return true;
            }

            var coreLib = typeof(object).Assembly.Location;
            if (string.IsNullOrWhiteSpace(coreLib)) { return false; }

            var directory = Path.GetDirectoryName(coreLib);
            while (directory != null)
            {
                if (Directory.Exists(Path.Combine(directory, "packs"))) { return true; }
                directory = Path.GetDirectoryName(directory);
            }

            return false;
        }
    }
}
