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
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace YamlDotNet.Analyzers.StaticGenerator
{
    [Generator]
    public class TypeFactoryGenerator : ISourceGenerator
    {
        private GeneratorExecutionContext _context;

        private static readonly DiagnosticDescriptor UnregisteredTypeDescriptor = new DiagnosticDescriptor(
            id: "YDNG001",
            title: "Unregistered type in YamlDotNet static context",
            messageFormat: "Property '{0}' on type '{1}' uses type '{2}' which is not registered in the YamlDotNet static context. Add [YamlSerializable(typeof({3}))] to your static context class.",
            category: "YamlDotNet.StaticGenerator",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

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

            ReportUnregisteredTypes(context, receiver);
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

                var namespaceName = syntaxReceiver.YamlStaticContextType?.ContainingNamespace.ContainingNamespace;

                write($"namespace {syntaxReceiver.YamlStaticContextType?.GetNamespace() ?? "YamlDotNet.Static"}", true);
                write("{", true); indent();

                new StaticContextFile(write, indent, unindent, _context).Write(syntaxReceiver);
                new StaticObjectFactoryFile(write, indent, unindent, _context).Write(syntaxReceiver);
                new StaticTypeResolverFile(write, indent, unindent, _context).Write(syntaxReceiver);
                new StaticPropertyDescriptorFile(write, indent, unindent, _context).Write(syntaxReceiver);
                new StaticTypeInspectorFile(write, indent, unindent, _context).Write(syntaxReceiver);
                new ObjectAccessorFileGenerator(write, indent, unindent, _context).Write(syntaxReceiver);

                unindent(); write("}", true);
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

        private static void ReportUnregisteredTypes(GeneratorExecutionContext context, SerializableSyntaxReceiver receiver)
        {
            // Build a set of all registered type full names (including collections auto-registered)
            var registeredTypeNames = new HashSet<string>(receiver.Classes.Keys);

            // Walk each registered class's properties and fields
            foreach (var entry in receiver.Classes)
            {
                var classObject = entry.Value;

                // Skip collection/array entries — they are auto-registered wrappers, not user-defined types
                if (classObject.IsArray || classObject.IsList || classObject.IsDictionary
                    || classObject.IsListOverride || classObject.IsDictionaryOverride)
                {
                    continue;
                }

                foreach (var property in classObject.PropertySymbols)
                {
                    CheckTypeRegistration(context, receiver, registeredTypeNames,
                        property.Type, property.Name, classObject.FullName, property.Locations);
                }

                foreach (var field in classObject.FieldSymbols)
                {
                    CheckTypeRegistration(context, receiver, registeredTypeNames,
                        field.Type, field.Name, classObject.FullName, field.Locations);
                }
            }
        }

        private static void CheckTypeRegistration(
            GeneratorExecutionContext context,
            SerializableSyntaxReceiver receiver,
            HashSet<string> registeredTypeNames,
            ITypeSymbol type,
            string memberName,
            string containingTypeName,
            System.Collections.Immutable.ImmutableArray<Location> locations)
        {
            // Unwrap nullable value types
            if (type is INamedTypeSymbol namedType
                && namedType.IsGenericType
                && namedType.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T)
            {
                type = namedType.TypeArguments[0];
            }

            // Skip types that don't need registration
            if (IsWellKnownType(type))
            {
                return;
            }

            // Check generic collection types — verify their element types
            var fullName = type.GetFullName().TrimEnd('?');
            if (fullName.StartsWith("System.Collections.Generic."))
            {
                if (type is INamedTypeSymbol genericType && genericType.IsGenericType)
                {
                    foreach (var typeArg in genericType.TypeArguments)
                    {
                        CheckTypeRegistration(context, receiver, registeredTypeNames,
                            typeArg, memberName, containingTypeName, locations);
                    }
                }
                return;
            }

            // Check array element types
            if (type is IArrayTypeSymbol arrayType)
            {
                CheckTypeRegistration(context, receiver, registeredTypeNames,
                    arrayType.ElementType, memberName, containingTypeName, locations);
                return;
            }

            // Check if the type is registered
            var sanitizedName = SanitizeName(fullName.TrimEnd('?'));
            if (!registeredTypeNames.Contains(sanitizedName))
            {
                var shortName = type.Name;
                var location = locations.FirstOrDefault() ?? Location.None;
                context.ReportDiagnostic(Diagnostic.Create(
                    UnregisteredTypeDescriptor,
                    location,
                    memberName,
                    containingTypeName,
                    fullName,
                    shortName));
            }
        }

        private static bool IsWellKnownType(ITypeSymbol type)
        {
            // Enums are handled by the enum mapping system
            if (type.TypeKind == TypeKind.Enum)
            {
                return true;
            }

            // Check for special types that don't need registration
            switch (type.SpecialType)
            {
                case SpecialType.System_Boolean:
                case SpecialType.System_Byte:
                case SpecialType.System_SByte:
                case SpecialType.System_Int16:
                case SpecialType.System_UInt16:
                case SpecialType.System_Int32:
                case SpecialType.System_UInt32:
                case SpecialType.System_Int64:
                case SpecialType.System_UInt64:
                case SpecialType.System_Single:
                case SpecialType.System_Double:
                case SpecialType.System_Decimal:
                case SpecialType.System_Char:
                case SpecialType.System_String:
                case SpecialType.System_Object:
                case SpecialType.System_DateTime:
                    return true;
            }

            // Check for other well-known BCL types that are handled
            // by built-in converters or the scalar deserializer
            var fullName = type.GetFullName().TrimEnd('?');
            switch (fullName)
            {
                case "System.Guid":
                case "System.TimeSpan":
                case "System.DateTimeOffset":
                case "System.Uri":
                case "System.Type":
#if NET6_0_OR_GREATER
                case "System.DateOnly":
                case "System.TimeOnly":
#endif
                    return true;
            }

            // DateOnly and TimeOnly may not be caught by the #if above since the
            // source generator itself targets netstandard2.0, so check by string
            if (fullName == "System.DateOnly" || fullName == "System.TimeOnly")
            {
                return true;
            }

            return false;
        }

        private static string SanitizeName(string name) =>
            new string(name.Select(c => char.IsLetterOrDigit(c) ? c : '_').ToArray());
    }
}
