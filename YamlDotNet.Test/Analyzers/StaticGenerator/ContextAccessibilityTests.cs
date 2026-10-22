// This file is part of YamlDotNet - A .NET library for YAML.
// Copyright (c) Antoine Aubry and contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
// of the Software, and to permit persons to whom the Software is furnished to do
// so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace YamlDotNet.Test.Analyzers.StaticGenerator
{
    /// <summary>
    /// Runs the static generator over a small compilation, because the generator accepts only one
    /// [YamlStaticContext] per compilation and the test assembly already uses it for a public context.
    /// </summary>
    public class ContextAccessibilityTests
    {
        private const string Namespace = "AccessibilityProbe";

        [Fact]
        public void InternalContextCompilesAndLeavesNoPublicTypes()
        {
            var (compilation, driverDiagnostics) = Generate("internal");

            driverDiagnostics.Where(IsProblem).Should().BeEmpty();
            compilation.GetDiagnostics().Where(IsProblem).Should().BeEmpty();

            GetContext(compilation).DeclaredAccessibility.Should().Be(Accessibility.Internal);
            PublicTypes(compilation).Should().BeEmpty();
        }

        [Fact]
        public void PublicContextCompilesAndOnlyExposesTheContext()
        {
            var (compilation, driverDiagnostics) = Generate("public");

            driverDiagnostics.Where(IsProblem).Should().BeEmpty();
            compilation.GetDiagnostics().Where(IsProblem).Should().BeEmpty();

            GetContext(compilation).DeclaredAccessibility.Should().Be(Accessibility.Public);
            PublicTypes(compilation).Should().Equal(Namespace + ".ProbeContext");
        }

        [Fact]
        public void GeneratedFallbackContextIsPublicWhenTheUserDeclaresNone()
        {
            var (compilation, driverDiagnostics) = GenerateFromSource(@"
using YamlDotNet.Serialization;

namespace AccessibilityProbe
{
    [YamlSerializable]
    public class ProbeDto
    {
        public string Name { get; set; }
    }
}");

            driverDiagnostics.Where(IsProblem).Should().BeEmpty();
            compilation.GetDiagnostics().Where(IsProblem).Should().BeEmpty();

            var context = compilation.GetTypeByMetadataName("StaticContext");
            context.Should().NotBeNull("the generator declares its own StaticContext when the user has none");
            context!.DeclaredAccessibility.Should().Be(Accessibility.Public);
            PublicTypes(compilation).Should().Equal("AccessibilityProbe.ProbeDto", "StaticContext");
        }

        private static (Compilation Compilation, IReadOnlyList<Diagnostic> DriverDiagnostics) Generate(string accessibility)
        {
            var source = $@"
using YamlDotNet.Serialization;

namespace {Namespace}
{{
    internal class ProbeDto
    {{
        public string Name {{ get; set; }}
    }}

    [YamlStaticContext]
    [YamlSerializable(typeof(ProbeDto))]
    {accessibility} partial class ProbeContext : StaticContext
    {{
    }}
}}";
            return GenerateFromSource(source);
        }

        private static (Compilation Compilation, IReadOnlyList<Diagnostic> DriverDiagnostics) GenerateFromSource(string source)
        {
            var references = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
                .Select(a => (MetadataReference)MetadataReference.CreateFromFile(a.Location))
                .ToList();

            var compilation = CSharpCompilation.Create(
                "AccessibilityProbe",
                new[] { CSharpSyntaxTree.ParseText(source) },
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            var driver = CSharpGeneratorDriver.Create(CreateGenerator())
                .RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);

            driver.GetRunResult().GeneratedTrees.Should().NotBeEmpty("the generator must have produced the context");
            return (output, diagnostics);
        }

        // The generator assembly is not strong named, so the signed test assembly cannot reference it at
        // compile time. The build copies it to generator/ and it is loaded from there.
        private static ISourceGenerator CreateGenerator()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "generator", "YamlDotNet.Analyzers.StaticGenerator.dll");
            var type = Assembly.LoadFrom(path).GetType("YamlDotNet.Analyzers.StaticGenerator.TypeFactoryGenerator", throwOnError: true)!;
            return (ISourceGenerator)Activator.CreateInstance(type)!;
        }

        private static bool IsProblem(Diagnostic diagnostic) => diagnostic.Severity >= DiagnosticSeverity.Warning;

        private static INamedTypeSymbol GetContext(Compilation compilation) =>
            compilation.GetTypeByMetadataName(Namespace + ".ProbeContext")!;

        private static IEnumerable<string> PublicTypes(Compilation compilation)
        {
            var result = new List<string>();
            Collect(compilation.Assembly.GlobalNamespace, result);
            return result.OrderBy(n => n, StringComparer.Ordinal);
        }

        private static void Collect(INamespaceOrTypeSymbol container, List<string> result)
        {
            foreach (var member in container.GetMembers())
            {
                if (member is INamespaceSymbol ns)
                {
                    Collect(ns, result);
                }
                else if (member is INamedTypeSymbol type)
                {
                    if (type.DeclaredAccessibility == Accessibility.Public)
                    {
                        result.Add(type.ToDisplayString());
                    }

                    Collect(type, result);
                }
            }
        }
    }
}
