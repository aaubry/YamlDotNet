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
    public class StaticContextFile : File
    {
        public StaticContextFile(Action<string, bool> write, Action indent, Action unindent, GeneratorExecutionContext context) : base(write, indent, unindent, context)
        {
        }

        public override void Write(SerializableSyntaxReceiver syntaxReceiver)
        {
            Write($"public partial class {syntaxReceiver.YamlStaticContextType?.Name ?? "StaticContext"} : YamlDotNet.Serialization.StaticContext");
            Write("{"); Indent();
            Write("private readonly YamlDotNet.Serialization.ObjectFactories.StaticObjectFactory _objectFactory;");
            Write("private readonly YamlDotNet.Serialization.ITypeResolver _typeResolver;");
            Write("private readonly YamlDotNet.Serialization.ITypeInspector _typeInspector;");
            Write($"public {syntaxReceiver.YamlStaticContextType?.Name ?? "StaticContext"}()");
            Write("{"); Indent();
            Write("_objectFactory = new StaticObjectFactory();");
            Write("_typeResolver = new StaticTypeResolver(this);");
            Write("_typeInspector = new StaticTypeInspector(_typeResolver);");
            UnIndent(); Write("}");
            Write("public override YamlDotNet.Serialization.ObjectFactories.StaticObjectFactory GetFactory() => _objectFactory;");
            Write("public override YamlDotNet.Serialization.ITypeInspector GetTypeInspector() => _typeInspector;");
            Write("public override YamlDotNet.Serialization.ITypeResolver GetTypeResolver() => _typeResolver;");
            Write("public override bool IsKnownType(Type type)");
            Write("{"); Indent();
            foreach (var o in syntaxReceiver.Classes)
            {
                var classObject = o.Value;
                if (classObject.IsArray || classObject.IsList || classObject.IsDictionary || classObject.IsDictionaryOverride || classObject.IsListOverride)
                {
                    Write($"if (type == typeof({classObject.ModuleSymbol.GetFullName().Replace("?", string.Empty)})) return true;");
                }
                else
                {
                    Write($"if (type == typeof({o.Value.ModuleSymbol.GetFullName().Replace("?", string.Empty)})) return true;");
                    //always support an array, ienumerable<T>, list and dictionary of the type
                    Write($"if (type == typeof({classObject.ModuleSymbol.GetFullName().Replace("?", string.Empty)}[])) return true;");
                    Write($"if (type == typeof(System.Collections.Generic.IEnumerable<{classObject.ModuleSymbol.GetFullName().Replace("?", string.Empty)}>)) return true;");
                    Write($"if (type == typeof(System.Collections.Generic.List<{classObject.ModuleSymbol.GetFullName().Replace("?", string.Empty)}>)) return true;");
                    Write($"if (type == typeof(System.Collections.Generic.Dictionary<string, {classObject.ModuleSymbol.GetFullName().Replace("?", string.Empty)}>)) return true;");
                }
            }
            // always support dictionary object
            Write("if (type == typeof(System.Collections.Generic.Dictionary<object, object>)) return true;");
            Write("return false;");
            UnIndent(); Write("}");
            UnIndent(); Write("}");
        }
    }
}
