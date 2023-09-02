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

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace YamlDotNet.Analyzers.StaticGenerator
{
    public class ClassSyntaxReceiver : ISyntaxContextReceiver
    {
        public List<string> Log { get; } = new();
        public Dictionary<string, ClassObject> Classes { get; } = new Dictionary<string, ClassObject>();
        public INamedTypeSymbol? YamlStaticContextType { get; set; }

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            if (context.Node is ClassDeclarationSyntax classDeclarationSyntax)
            {
                var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax)!;
                if (classSymbol.GetAttributes().Any())
                {
                    var attributes = classSymbol.GetAttributes();
                    if (attributes.Any(attribute => attribute.AttributeClass?.ToDisplayString() == "YamlDotNet.Serialization.YamlStaticContextAttribute"))
                    {
                        YamlStaticContextType = classSymbol;
                    }

                    if (classSymbol.GetAttributes().Any(attribute => attribute.AttributeClass?.ToDisplayString() == "YamlDotNet.Serialization.YamlSerializableAttribute"))
                    {
                        ClassObject classObject;
                        var className = SanitizeName(classSymbol.GetFullName());
                        if (Classes.ContainsKey(className))
                        {
                            classObject = Classes[className];
                        }
                        else
                        {
                            classObject = new ClassObject(className, classSymbol);
                            Classes[className] = classObject;
                        }
                        while (classSymbol != null)
                        {
                            var members = classSymbol.GetMembers();
                            foreach (var member in members)
                            {
                                if (member.IsStatic ||
                                    (member.DeclaredAccessibility != Accessibility.Public &&
                                     member.DeclaredAccessibility != Accessibility.Internal) ||
                                    member.GetAttributes().Any(x => x.AttributeClass!.ToDisplayString() == "YamlDotNet.Serialization.YamlIgnoreAttribute"))
                                {
                                    continue;
                                }

                                if (member is IPropertySymbol propertySymbol)
                                {
                                    classObject.PropertySymbols.Add(propertySymbol);
                                    CheckForSupportedGeneric(propertySymbol.Type);
                                }
                                else if (member is IFieldSymbol fieldSymbol)
                                {
                                    classObject.FieldSymbols.Add(fieldSymbol);
                                    CheckForSupportedGeneric(fieldSymbol.Type);
                                }
                                else if (member is IMethodSymbol methodSymbol)
                                {
                                    var methodAttributes = methodSymbol.GetAttributes();
                                    if (methodAttributes.Any(x => x.AttributeClass!.ToDisplayString() == "YamlDotNet.Serialization.Callbacks.OnDeserializedAttribute"))
                                    {
                                        classObject.OnDeserializedMethods.Add(methodSymbol);
                                    }
                                    if (methodAttributes.Any(x => x.AttributeClass!.ToDisplayString() == "YamlDotNet.Serialization.Callbacks.OnDeserializingAttribute"))
                                    {
                                        classObject.OnDeserializingMethods.Add(methodSymbol);
                                    }
                                    if (methodAttributes.Any(x => x.AttributeClass!.ToDisplayString() == "YamlDotNet.Serialization.Callbacks.OnSerializedAttribute"))
                                    {
                                        classObject.OnSerializedMethods.Add(methodSymbol);
                                    }
                                    if (methodAttributes.Any(x => x.AttributeClass!.ToDisplayString() == "YamlDotNet.Serialization.Callbacks.OnSerializingAttribute"))
                                    {
                                        classObject.OnSerializingMethods.Add(methodSymbol);
                                    }
                                }
                            }
                            classSymbol = classSymbol.BaseType;
                        }
                    }
                }
            }
        }

        private string SanitizeName(string name) => new string(name.Select(c => char.IsLetterOrDigit(c) ? c : '_').ToArray());

        private void CheckForSupportedGeneric(ITypeSymbol type)
        {
            var typeName = type.GetFullName();
            var sanitizedTypeName = SanitizeName(typeName);

            if (Classes.ContainsKey(sanitizedTypeName))
            {
                return;
            }

            if (type.Kind == SymbolKind.ArrayType)
            {
                Classes.Add(sanitizedTypeName, new ClassObject(sanitizedTypeName, (IArrayTypeSymbol)type, isArray: true));
            }
            else if (typeName.StartsWith("System.Collections.Generic.Dictionary"))
            {
                Classes.Add(sanitizedTypeName, new ClassObject(sanitizedTypeName, (INamedTypeSymbol)type, true));
            }
            else if (typeName.StartsWith("System.Collections.Generic.List"))
            {
                Classes.Add(sanitizedTypeName, new ClassObject(sanitizedTypeName, (INamedTypeSymbol)type, isList: true));
            }
        }
    }
}
