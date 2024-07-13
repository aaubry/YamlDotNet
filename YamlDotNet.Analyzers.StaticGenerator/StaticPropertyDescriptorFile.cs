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
using Microsoft.CodeAnalysis;

namespace YamlDotNet.Analyzers.StaticGenerator
{
    public class StaticPropertyDescriptorFile : File
    {
        public StaticPropertyDescriptorFile(Action<string, bool> write, Action indent, Action unindent, GeneratorExecutionContext context) : base(write, indent, unindent, context)
        {
        }

        public override void Write(SerializableSyntaxReceiver syntaxReceiver)
        {
            Write("class StaticPropertyDescriptor : YamlDotNet.Serialization.IPropertyDescriptor");
            Write("{"); Indent();
            Write("private readonly YamlDotNet.Serialization.ITypeResolver _typeResolver;");
            Write("private YamlDotNet.Serialization.IObjectAccessor _accessor;");
            Write("private readonly Attribute[] _attributes;");
            Write("public string Name { get; }");
            Write("public bool CanWrite { get; }");
            Write("public Type Type { get; }");
            Write("public Type ConverterType { get; }");
            Write("public Type TypeOverride { get; set; }");
            Write("public bool AllowNulls { get; set; }");
            Write("public bool Required { get; }");
            Write("public int Order { get; set; }");
            Write("public YamlDotNet.Core.ScalarStyle ScalarStyle { get; set; }");
            Write("public T GetCustomAttribute<T>() where T : Attribute");
            Write("{"); Indent();
            Write("foreach (var attribute in this._attributes)");
            Write("{"); Indent();
            Write("if (attribute.GetType() == typeof(T)) { return (T)attribute; }");
            UnIndent(); Write("}");
            Write("return null;");
            UnIndent(); Write("}");
            Write("public YamlDotNet.Serialization.IObjectDescriptor Read(object target)");
            Write("{"); Indent();
            Write("var propertyValue = _accessor.Read(Name, target);");
            Write("var actualType = _typeResolver.Resolve(Type, propertyValue);");
            Write("return new YamlDotNet.Serialization.ObjectDescriptor(propertyValue, actualType, Type, this.ScalarStyle);");
            UnIndent(); Write("}");
            Write("public void Write(object target, object value)");
            Write("{"); Indent();
            Write("_accessor.Set(Name, target, value);");
            UnIndent(); Write("}");
            Write("public StaticPropertyDescriptor(YamlDotNet.Serialization.ITypeResolver typeResolver, YamlDotNet.Serialization.IObjectAccessor accessor, string name, bool canWrite, Type type, Attribute[] attributes, bool allowNulls, bool isRequired, Type converterType)");
            Write("{"); Indent();
            Write("this._typeResolver = typeResolver;");
            Write("this._accessor = accessor;");
            Write("this._attributes = attributes;");
            Write("this.Name = name;");
            Write("this.CanWrite = canWrite;");
            Write("this.Type = type;");
            Write("this.ConverterType = converterType;");
            Write("this.ScalarStyle = YamlDotNet.Core.ScalarStyle.Any;");
            Write("this.AllowNulls = allowNulls;");
            Write("this.Required = isRequired;");
            UnIndent(); Write("}");
            UnIndent(); Write("}");
        }
    }
}
