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
using System.Text;
using Microsoft.CodeAnalysis;

namespace YamlDotNet.Analyzers.StaticGenerator
{
    public class ObjectAccessorFileGenerator : File
    {
        public ObjectAccessorFileGenerator(Action<string, bool> Write, Action indent, Action unindent, GeneratorExecutionContext context) : base(Write, indent, unindent, context)
        {
        }

        public override void Write(SerializableSyntaxReceiver syntaxReceiver)
        {
            foreach (var o in syntaxReceiver.Classes)
            {
                var classObject = o.Value;
                Write($"class {classObject.SanitizedClassName}_{classObject.GuidSuffix} : YamlDotNet.Serialization.IObjectAccessor");
                Write("{"); Indent();

                Write("public void Set(string propertyName, object target, object value)");
                Write("{"); Indent();
                var typeName = classObject.FullName.Replace("?", string.Empty);
                if (classObject.FieldSymbols.Count > 0 || classObject.PropertySymbols.Count > 0)
                {
                    if (classObject.ModuleSymbol.TypeKind is TypeKind.Struct) {
                        Write($"ref var v = ref System.Runtime.CompilerServices.Unsafe.Unbox<{typeName}>(target);");
                    }
                    else {
                        Write($"var v = ({typeName})target;");
                    }
                    Write("switch (propertyName)");
                    Write("{"); Indent();
                    foreach (var field in classObject.FieldSymbols)
                    {
                        if (!field.IsReadOnly)
                        {
                            Write(GetSetter(field.Name, field.Type));
                        }
                    }
                    foreach (var property in classObject.PropertySymbols)
                    {
                        if (property.SetMethod != null)
                        {
                            Write(GetSetter(property.Name, property.Type));
                        }
                    }
                    Write("default: throw new ArgumentOutOfRangeException(\"propertyName\", $\"{propertyName} does not exist or is not settable\");");
                    UnIndent(); Write("}");
                }
                UnIndent(); Write("}");

                Write("public object Read(string propertyName, object target)");
                Write("{"); Indent();
                Write($"var v = ({typeName})target;");
                if (classObject.FieldSymbols.Count > 0 || classObject.PropertySymbols.Count > 0)
                {
                    Write("switch (propertyName)");
                    Write("{"); Indent();
                    foreach (var field in classObject.FieldSymbols)
                    {
                        Write(GetReader(field.Name));
                    }
                    foreach (var property in classObject.PropertySymbols)
                    {
                        Write(GetReader(property.Name));
                    }
                    UnIndent(); Write("}");
                }
                Write("return null;");
                UnIndent(); Write("}");
                UnIndent(); Write("}");
            }
        }

        private string GetSetter(string name, ITypeSymbol type)
        {
            return $"case \"{name}\": v.{name} = ({type.GetFullName().Replace("?", string.Empty)})value; return;";
        }

        private string GetReader(string name)
        {
            return $"case \"{name}\": return v.{name};";
        }
    }
}
