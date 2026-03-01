using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.XPath;
using Cuemon;
using Cuemon.Reflection;

namespace Codebelt.Extensions.Swashbuckle.AspNetCore
{
    /// <summary>
    /// Extension methods for the <see cref="XPathDocument"/> class.
    /// </summary>
    public static class XPathDocumentExtensions
    {
        /// <summary>
        /// Adds the specified <paramref name="type"/> to the collection of <paramref name="documents"/>.
        /// </summary>
        /// <param name="documents">The collection of documents in XML format.</param>
        /// <param name="type">The type to locate XML documentation files by.</param>
        /// <returns>A reference to <paramref name="documents" /> so that additional calls can be chained.</returns>
        public static IList<XPathDocument> AddByType(this IList<XPathDocument> documents, Type type)
        {
            Validator.ThrowIfNull(type);
            return AddByAssembly(documents, type.Assembly);
        }

        /// <summary>
        /// Adds the assembly of <typeparamref name="T"/> to the collection of <paramref name="documents"/>.
        /// </summary>
        /// <typeparam name="T">The type to locate XML documentation files by.</typeparam>
        /// <param name="documents">The collection of documents in XML format.</param>
        /// <returns>A reference to <paramref name="documents" /> so that additional calls can be chained.</returns>
        public static IList<XPathDocument> AddByType<T>(this IList<XPathDocument> documents)
        {
            return AddByType(documents, typeof(T));
        }

        /// <summary>
        /// Adds the specified <paramref name="assembly"/> to the collection of <paramref name="documents"/>.
        /// </summary>
        /// <param name="documents">The collection of documents in XML format.</param>
        /// <param name="assembly">The assembly to locate XML documentation files by.</param>
        /// <returns>A reference to <paramref name="documents" /> so that additional calls can be chained.</returns>
        public static IList<XPathDocument> AddByAssembly(this IList<XPathDocument> documents, Assembly assembly)
        {
            Validator.ThrowIfNull(assembly);
            if (assembly.IsDynamic || string.IsNullOrWhiteSpace(assembly.Location)) { return documents; }
            var path = Path.ChangeExtension(assembly.Location, ".xml");
            return AddByFilename(documents, path);
        }

        /// <summary>
        /// Adds the specified <paramref name="path"/> to the collection of <paramref name="documents"/>.
        /// </summary>
        /// <param name="documents">The collection of documents in XML format.</param>
        /// <param name="path">The path to locate XML documentation files by.</param>
        /// <returns>A reference to <paramref name="documents" /> so that additional calls can be chained.</returns>
        public static IList<XPathDocument> AddByFilename(this IList<XPathDocument> documents, string path)
        {
            Validator.ThrowIfNull(documents);
            Validator.ThrowIfNullOrWhitespace(path);
            if (File.Exists(path))
            {
                documents.Add(new XPathDocument(path));
            }
            return documents;
        }

        /// <summary>
        /// Adds XML documentation files from <see cref="AppContext.BaseDirectory"/> that match the assembly of the specified <paramref name="type"/> and its base types.
        /// </summary>
        /// <param name="documents">The collection of documents in XML format.</param>
        /// <param name="type">The type whose assembly (and base type assemblies) is used to locate XML documentation files in the base directory.</param>
        /// <returns>A reference to <paramref name="documents" /> so that additional calls can be chained.</returns>
        public static IList<XPathDocument> AddFromBaseDirectory(this IList<XPathDocument> documents, Type type)
        {
            Validator.ThrowIfNull(documents);
            Validator.ThrowIfNull(type);
            var hierarchyAssemblyNames = GetTypeHierarchyAssemblies(type)
                .Select(a => a.GetName().FullName)
                .ToHashSet();
            var domainAssemblies = AssemblyContext.GetCurrentDomainAssemblies(o =>
            {
                o.AssemblyFilter = _ => true;
                o.ReferencedAssemblyFilter = _ => true;
            }).Where(ass => hierarchyAssemblyNames.Contains(ass.GetName().FullName)).Select(ass => ass.GetName().Name).ToList();
            foreach (var file in GetBaseDirectoryXmlFiles().Where(filename => domainAssemblies.Contains(Path.GetFileNameWithoutExtension(filename))))
            {
                AddByFilename(documents, file);
            }
            return documents;
        }

        /// <summary>
        /// Adds XML documentation files from <see cref="AppContext.BaseDirectory"/> that match the assembly of <typeparamref name="T"/> and its base types.
        /// </summary>
        /// <typeparam name="T">The type whose assembly (and base type assemblies) is used to locate XML documentation files in the base directory.</typeparam>
        /// <param name="documents">The collection of documents in XML format.</param>
        /// <returns>A reference to <paramref name="documents" /> so that additional calls can be chained.</returns>
        public static IList<XPathDocument> AddFromBaseDirectory<T>(this IList<XPathDocument> documents)
        {
            return AddFromBaseDirectory(documents, typeof(T));
        }

        /// <summary>
        /// Adds XML documentation files from installed .NET reference packs that match the assembly of the specified <paramref name="type"/> and its base types.
        /// </summary>
        /// <param name="documents">The collection of documents in XML format.</param>
        /// <param name="type">The type whose assembly (and base type assemblies) is used to locate XML documentation files in the .NET reference packs.</param>
        /// <returns>A reference to <paramref name="documents" /> so that additional calls can be chained.</returns>
        public static IList<XPathDocument> AddFromReferencePacks(this IList<XPathDocument> documents, Type type)
        {
            Validator.ThrowIfNull(documents);
            Validator.ThrowIfNull(type);
            var packsDirectory = GetDotnetPacksDirectory();
            if (packsDirectory == null) { return documents; }
            var tfm = $"net{Environment.Version.Major}.{Environment.Version.Minor}";
            foreach (var assembly in GetTypeHierarchyAssemblies(type))
            {
                var assemblyName = assembly.GetName().Name;
                foreach (var refPackDir in Directory.EnumerateDirectories(packsDirectory, "*.Ref"))
                {
                    var found = false;
                    foreach (var versionDir in Directory.EnumerateDirectories(refPackDir))
                    {
                        var xmlPath = Path.Combine(versionDir, "ref", tfm, $"{assemblyName}.xml");
                        if (File.Exists(xmlPath))
                        {
                            AddByFilename(documents, xmlPath);
                            found = true;
                            break;
                        }
                    }
                    if (found) { break; }
                }
            }
            return documents;
        }

        /// <summary>
        /// Adds XML documentation files from installed .NET reference packs that match the assembly of <typeparamref name="T"/> and its base types.
        /// </summary>
        /// <typeparam name="T">The type whose assembly (and base type assemblies) is used to locate XML documentation files in the .NET reference packs.</typeparam>
        /// <param name="documents">The collection of documents in XML format.</param>
        /// <returns>A reference to <paramref name="documents" /> so that additional calls can be chained.</returns>
        public static IList<XPathDocument> AddFromReferencePacks<T>(this IList<XPathDocument> documents)
        {
            return AddFromReferencePacks(documents, typeof(T));
        }

        private static IEnumerable<string> GetBaseDirectoryXmlFiles()
        {
            foreach (var file in Directory.EnumerateFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly))
            {
                yield return file;
            }
        }

        private static string GetDotnetPacksDirectory()
        {
            var dotnetRoot = GetDotnetRootDirectory();
            if (dotnetRoot == null) { return null; }
            var packsDir = Path.Combine(dotnetRoot, "packs");
            return Directory.Exists(packsDir) ? packsDir : null;
        }

        private static string GetDotnetRootDirectory()
        {
            var dotnetRoot = Environment.GetEnvironmentVariable("DOTNET_ROOT");
            if (!string.IsNullOrWhiteSpace(dotnetRoot) && Directory.Exists(dotnetRoot)) { return dotnetRoot; }
            var coreLib = typeof(object).Assembly.Location;
            if (string.IsNullOrWhiteSpace(coreLib)) { return null; }
            var directory = Path.GetDirectoryName(coreLib);
            while (directory != null)
            {
                if (Directory.Exists(Path.Combine(directory, "packs"))) { return directory; }
                directory = Path.GetDirectoryName(directory);
            }
            return null;
        }

        private static IEnumerable<Assembly> GetTypeHierarchyAssemblies(Type type)
        {
            var seen = new HashSet<string>();
            var current = type;
            while (current != null)
            {
                var assembly = current.Assembly;
                if (!assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
                {
                    if (seen.Add(assembly.GetName().FullName))
                    {
                        yield return assembly;
                    }
                }
                current = current.BaseType;
            }
        }
    }
}
