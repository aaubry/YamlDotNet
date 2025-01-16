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
using System.Text;
using Microsoft.CodeAnalysis;

namespace YamlDotNet.Analyzers.StaticGenerator
{
    [Generator]
    public class TypeFactoryGenerator : ISourceGenerator
    {
        private GeneratorExecutionContext _context;

        public void Execute(GeneratorExecutionContext context)
        {
            if (!(context.SyntaxContextReceiver is SerializableSyntaxReceiver receiver))
            {
                return;
            }

            if (receiver.Classes.Count == 0)
            {
                return;
            }

            _context = context;

            foreach (var log in receiver.Log)
            {
                context.ReportDiagnostic(Diagnostic.Create("1", typeof(TypeFactoryGenerator).FullName, log, DiagnosticSeverity.Warning, DiagnosticSeverity.Warning, true, 1));
            }

            context.AddSource("YamlDotNetAutoGraph.g.cs", GenerateSource(receiver));
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            var syntaxReceiver = new SerializableSyntaxReceiver();
            context.RegisterForSyntaxNotifications(() => syntaxReceiver);
        }

        private string GenerateSource(SerializableSyntaxReceiver syntaxReceiver)
        {
            var result = new StringBuilder();
            var indentation = 0;
            var indent = new Action(() => indentation++);
            var unindent = new Action(() => indentation--);

            var write = new Action<string, bool>((string line, bool includeNewLine) =>
            {
                if (indentation > 0)
                {
                    result.Append(new string(' ', indentation * 4));
                }
                result.Append(line);
                if (includeNewLine)
                {
                    result.AppendLine();
                }
            });

            try
            {
                write("#pragma warning disable CS8767 // Nullability of reference types", true);
                write("#pragma warning disable CS8767 // Nullability of reference types", true);
                write("#pragma warning disable CS8603 // Possible null reference return", true);
                write("#pragma warning disable CS8604 // Possible null reference argument", true);
                write("#pragma warning disable CS8766 // Nullability of reference types", true);

                write("using System;", true);
                write("using System.Collections.Generic;", true);

                var namespaceName = syntaxReceiver.YamlStaticContextType?.ContainingNamespace?.Name;
                var isGlobalNamespace = string.IsNullOrEmpty(namespaceName);

                if (!isGlobalNamespace)
                {
                    write($"namespace {namespaceName}", true);
                    write("{", true); indent();
                }

                new StaticContextFile(write, indent, unindent, _context).Write(syntaxReceiver);
                new StaticObjectFactoryFile(write, indent, unindent, _context).Write(syntaxReceiver);
                new StaticTypeResolverFile(write, indent, unindent, _context).Write(syntaxReceiver);
                new StaticPropertyDescriptorFile(write, indent, unindent, _context).Write(syntaxReceiver);
                new StaticTypeInspectorFile(write, indent, unindent, _context).Write(syntaxReceiver);
                new ObjectAccessorFileGenerator(write, indent, unindent, _context).Write(syntaxReceiver);

                if (!isGlobalNamespace)
                {
                    unindent(); write("}", true);
                }
            }
            catch (Exception exception)
            {
                write("/*", true);
                var e = exception;
                while (e != null)
                {
                    write(exception.Message, true);
                    write(exception.StackTrace, true);
                    write("======", true);
                    e = exception.InnerException;
                }
                write("*/", true);
            }
            return result.ToString();
        }
    }
}
