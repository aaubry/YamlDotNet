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

namespace YamlDotNet.Analyzers
{
    public class ClassSyntaxReceiver : ISyntaxContextReceiver
    {
        public List<string> Log { get; } = new();
        public Dictionary<string, ClassObject> Classes { get; } = new Dictionary<string, ClassObject>();

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            if (context.Node is ClassDeclarationSyntax classDeclarationSyntax)
            {
                var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax)!;
                if (classSymbol.GetAttributes().Any())
                {
                    if (classSymbol.GetAttributes().Any(attribute => attribute.AttributeClass?.ToDisplayString() == "YamlDotNet.Serialization.YamlSerializableAttribute"))
                    {
                        ClassObject classObject;
                        var className = $"{classSymbol.GetFullName().Replace(".", "_")}";
                        if (Classes.ContainsKey(className))
                        {
                            classObject = Classes[className];
                        }
                        else
                        {
                            classObject = new ClassObject(className, classSymbol);
                            Classes[className] = classObject;
                        }
                        var members = classSymbol.GetMembers();
                        foreach (var member in members)
                        {
                            if (member.IsStatic ||
                                member.DeclaredAccessibility != Accessibility.Public ||
                                member.GetAttributes().Any(x => x.AttributeClass!.ToDisplayString() == "YamlDotNet.Serialization.YamlIgnoreAttribute"))
                            {
                                continue;
                            }

                            if (member is IPropertySymbol propertySymbol)
                            {
                                classObject.PropertySymbols.Add(propertySymbol);
                            }
                            else if (member is IFieldSymbol fieldSymbol)
                            {
                                classObject.FieldSymbols.Add(fieldSymbol);
                            }
                        }
                    }
                }
            }
        }
    }
}
