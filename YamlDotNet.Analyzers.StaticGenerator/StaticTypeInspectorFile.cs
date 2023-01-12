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
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace YamlDotNet.Analyzers.StaticGenerator
{
    public class StaticTypeInspectorFile : File
    {
        public StaticTypeInspectorFile(Action<string> Write, Action indent, Action unindent, GeneratorExecutionContext context) : base(Write, indent, unindent, context)
        {
        }

        public override void Write(ClassSyntaxReceiver classSyntaxReceiver)
        {
            Write("public class StaticTypeInspector : YamlDotNet.Serialization.ITypeInspector");
            Write("{"); Indent();

            #region GetProperties
            Write("public IEnumerable<YamlDotNet.Serialization.IPropertyDescriptor> GetProperties(Type type, object container)");
            Write("{"); Indent();
            foreach (var o in classSyntaxReceiver.Classes)
            {
                var classObject = o.Value;
                Write($"if (type == typeof({classObject.ModuleSymbol.GetFullName()}))");
                Write("{"); Indent();
                Write($"var accessor = new {classObject.SanitizedClassName}_{classObject.GuidSuffix}();");
                Write("return new YamlDotNet.Serialization.IPropertyDescriptor[]");
                Write("{"); Indent();
                foreach (var field in classObject.FieldSymbols)
                {
                    Write(GetPropertyDescriptor(field.Name, field.Type, !field.IsReadOnly, field.GetAttributes(), ','));
                }
                foreach (var property in classObject.PropertySymbols)
                {
                    Write(GetPropertyDescriptor(property.Name, property.Type, property.SetMethod == null, property.GetAttributes(), ','));
                }
                UnIndent(); Write("};");
                UnIndent(); Write("}");
            }
            Write("return new List<YamlDotNet.Serialization.IPropertyDescriptor>();");
            UnIndent(); Write("}");
            #endregion

            #region GetProperty
            Write("public YamlDotNet.Serialization.IPropertyDescriptor GetProperty(Type type, object container, string name, bool ignoreUnmatched)");
            Write("{"); Indent();
            foreach (var o in classSyntaxReceiver.Classes)
            {
                var classObject = o.Value;
                Write($"if (type == typeof({classObject.ModuleSymbol.GetFullName()}))");
                Write("{"); Indent();
                Write($"var accessor = new {classObject.SanitizedClassName}_{classObject.GuidSuffix}();");
                foreach (var field in classObject.FieldSymbols)
                {
                    Write($"if (name == \"{field.Name}\") return {GetPropertyDescriptor(field.Name, field.Type, field.IsReadOnly, field.GetAttributes(), ';')}");
                }
                foreach (var property in classObject.PropertySymbols)
                {
                    Write($"if (name == \"{property.Name}\") return {GetPropertyDescriptor(property.Name, property.Type, property.SetMethod == null, property.GetAttributes(), ';')}");
                }
                UnIndent(); Write("}");

            }
            Write("if (ignoreUnmatched) return null;");
            Write("throw new ArgumentOutOfRangeException(\"Field or property not valid: \" + name);");
            UnIndent(); Write("}");
            #endregion

            UnIndent(); Write("}");
        }

        private string GetPropertyDescriptor(string name, ITypeSymbol type, bool isReadonly, ImmutableArray<AttributeData> attributes, char finalChar)
        {
            var constructor = new StringBuilder($"new StaticPropertyDescriptor(accessor, \"{name}\", {(!isReadonly).ToString().ToLower()}, typeof({type.GetFullName()}), new Attribute[] {{");
            constructor.AppendLine();
            foreach (var attribute in attributes)
            {
                switch (attribute.AttributeClass?.ToDisplayString())
                {
                    case "YamlDotNet.Serialization.YamlIgnore":
                        Write("new YamlDotNet.Serialization.YamlIgnoreAttribute(),");
                        break;
                    case "YamlDotNet.Serialization.YamlMemberAttribute":
                        constructor.Append("new YamlDotNet.Serialization.YamlMemberAttribute() {");
                        for (var index = 0; index < attribute.NamedArguments.Length; index++)
                        {
                            if (index > 0)
                            {
                                constructor.Append(", ");
                            }
                            var attributeData = attribute.NamedArguments[index];
                            switch (attributeData.Key)
                            {
                                case "Description":
                                    constructor.Append($"Description = {attributeData.Value.ToCSharpString()}");
                                    break;
                                case "Order":
                                    constructor.Append($"Order = {attributeData.Value.Value}");
                                    break;
                                case "Alias":
                                    constructor.Append($"Alias = {attributeData.Value.ToCSharpString()}");
                                    break;
                                case "ApplyNamingConventions":
                                    constructor.Append($"ApplyNamingConventions = {attributeData.Value.ToCSharpString()}");
                                    break;
                                case "DefaultValuesHandling":
                                    constructor.Append($"DefaultValuesHandling = (YamlDotNet.Serialization.DefaultValuesHandling){attributeData.Value.Value}");
                                    break;
                                case "ScalarStyle":
                                    constructor.Append($"ScalarStyle = (YamlDotNet.Core.ScalarStyle){attributeData.Value.Value}");
                                    break;
                            }
                        }
                        constructor.AppendLine("},");
                        break;
                }
            }
            constructor.Append("})");
            constructor.Append(finalChar.ToString());
            return constructor.ToString();
        }
    }
}
