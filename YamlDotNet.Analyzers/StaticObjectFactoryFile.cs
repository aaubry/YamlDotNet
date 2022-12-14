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

namespace YamlDotNet.Analyzers
{
    public class StaticObjectFactoryFile : File
    {
        public StaticObjectFactoryFile(Action<string> write, Action indent, Action unindent, GeneratorExecutionContext context) : base(write, indent, unindent, context)
        {
        }

        public override void Write(ClassSyntaxReceiver classSyntaxReceiver)
        {
            Write("public class StaticObjectFactory : YamlDotNet.Serialization.ObjectFactories.StaticObjectFactory");
            Write("{"); Indent();

            Write("public override object Create(Type type)");
            Write("{"); Indent();
            foreach (var o in classSyntaxReceiver.Classes)
            {
                var classObject = o.Value;
                Write($"if (type == typeof({classObject.ModuleSymbol.GetFullName()})) return new {classObject.ModuleSymbol.GetFullName()}();");
            }
            Write($"throw new ArgumentOutOfRangeException(\"Unknown type: \" + type.ToString());");
            UnIndent(); Write("}");

            Write("public override bool IsDictionary(Type type)");
            Write("{"); Indent();
            foreach (var o in classSyntaxReceiver.Classes)
            {
                var classObject = o.Value;
                Write($"if (type == typeof({classObject.ModuleSymbol.GetFullName()})) return {classObject.IsDictionary.ToString().ToLower()};");
            }
            Write("return false;");
            UnIndent(); Write("}");

            Write("public override bool IsList(Type type)");
            Write("{"); Indent();
            foreach (var o in classSyntaxReceiver.Classes)
            {
                var classObject = o.Value;
                Write($"if (type == typeof({classObject.ModuleSymbol.GetFullName()})) return {classObject.IsList.ToString().ToLower()};");
            }
            Write("return false;");
            UnIndent(); Write("}");

            Write("public override Type GetKeyType(Type type)");
            Write("{"); Indent();
            foreach (var o in classSyntaxReceiver.Classes)
            {
                var classObject = o.Value;
                if (!classObject.IsDictionary)
                {
                    continue;
                }

                var keyType = "object";
                if (classObject.ModuleSymbol.IsGenericType)
                {
                    keyType = o.Value.ModuleSymbol.TypeArguments[0].GetFullName();
                }
                Write($"if (type == typeof({classObject.ModuleSymbol.GetFullName()})) return typeof({keyType});");
            }
            Write("throw new ArgumentOutOfRangeException(\"Unknown type: \" + type.ToString());");
            UnIndent(); Write("}");

            Write("public override Type GetValueType(Type type)");
            Write("{"); Indent();
            foreach (var o in classSyntaxReceiver.Classes)
            {
                var classObject = o.Value;
                if (!(classObject.IsList || classObject.IsDictionary))
                {
                    continue;
                }

                string valueType;
                if (classObject.IsDictionary)
                {
                    //we're a dictionary
                    valueType = o.Value.ModuleSymbol.TypeArguments[1].GetFullName();
                }
                else
                {
                    //we're a list
                    valueType = o.Value.ModuleSymbol.TypeArguments[0].GetFullName();
                }

                Write($"if (type == typeof({classObject.ModuleSymbol.GetFullName()})) return typeof({valueType});");
            }
            Write("throw new ArgumentOutOfRangeException(\"Unknown type: \" + type.ToString());");
            UnIndent(); Write("}");

            UnIndent(); Write("}");
        }
    }
}
