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
using Microsoft.CodeAnalysis;

namespace YamlDotNet.Analyzers.StaticGenerator
{
    public class StaticObjectFactoryFile : File
    {
        public StaticObjectFactoryFile(Action<string, bool> write, Action indent, Action unindent, GeneratorExecutionContext context) : base(write, indent, unindent, context)
        {
        }

        public override void Write(ClassSyntaxReceiver classSyntaxReceiver)
        {
            Write($"class StaticObjectFactory : YamlDotNet.Serialization.ObjectFactories.StaticObjectFactory");
            Write("{"); Indent();

            Write("public override object Create(Type type)");
            Write("{"); Indent();
            foreach (var o in classSyntaxReceiver.Classes.Where(c => !c.Value.IsArray))
            {
                var classObject = o.Value;
                Write($"if (type == typeof({classObject.ModuleSymbol.GetFullName().Replace("?", string.Empty)})) return new {classObject.ModuleSymbol.GetFullName().Replace("?", string.Empty)}();");
                Write($"if (type == typeof(System.Collections.Generic.List<{classObject.ModuleSymbol.GetFullName().Replace("?", string.Empty)}>)) return new System.Collections.Generic.List<{classObject.ModuleSymbol.GetFullName().Replace("?", string.Empty)}>();");
            }
            Write($"throw new ArgumentOutOfRangeException(\"Unknown type: \" + type.ToString());");
            UnIndent(); Write("}");

            Write("public override Array CreateArray(Type type, int count)");
            Write("{"); Indent();
            foreach (var o in classSyntaxReceiver.Classes)
            {
                var classObject = o.Value;
                if (classObject.IsArray)
                {
                    Write($"if (type == typeof({classObject.ModuleSymbol.GetFullName().Replace("?", string.Empty)})) return new {classObject.ModuleSymbol.GetFullName(false).Replace("?", string.Empty)}[count];");
                }
                else
                {
                    Write($"if (type == typeof({classObject.ModuleSymbol.GetFullName().Replace("?", string.Empty)}[])) return new {classObject.ModuleSymbol.GetFullName(false).Replace("?", string.Empty)}[count];");
                }
            }
            Write($"throw new ArgumentOutOfRangeException(\"Unknown type: \" + type.ToString());");
            UnIndent(); Write("}");

            Write("public override bool IsDictionary(Type type)");
            Write("{"); Indent();
            foreach (var o in classSyntaxReceiver.Classes.Where(c => c.Value.IsDictionary))
            {
                Write($"if (type == typeof({o.Value.ModuleSymbol.GetFullName().Replace("?", string.Empty)})) return true;");
            }
            Write("return false;");
            UnIndent(); Write("}");

            Write("public override bool IsArray(Type type)");
            Write("{"); Indent();
            foreach (var o in classSyntaxReceiver.Classes)
            {
                var classObject = o.Value;
                if (o.Value.IsArray)
                {
                    Write($"if (type == typeof({classObject.ModuleSymbol.GetFullName().Replace("?", string.Empty)})) return true;");
                }
                else
                {
                    //always support an array of the type
                    Write($"if (type == typeof({classObject.ModuleSymbol.GetFullName().Replace("?", string.Empty)}[])) return true;");
                }
            }
            Write("return false;");
            UnIndent(); Write("}");

            Write("public override bool IsList(Type type)");
            Write("{"); Indent();
            foreach (var o in classSyntaxReceiver.Classes)
            {
                var classObject = o.Value;
                if (classObject.IsList)
                {
                    Write($"if (type == typeof({classObject.ModuleSymbol.GetFullName().Replace("?", string.Empty)})) return true;");
                }
                else
                {
                    //always support a list of the type
                    Write($"if (type == typeof(System.Collections.Generic.List<{classObject.ModuleSymbol.GetFullName().Replace("?", string.Empty)}>)) return true;");
                }
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
                var type = (INamedTypeSymbol)classObject.ModuleSymbol;

                if (type.IsGenericType)
                {
                    keyType = type.TypeArguments[0].GetFullName().Replace("?", string.Empty);
                }
                Write($"if (type == typeof({classObject.ModuleSymbol.GetFullName().Replace("?", string.Empty)})) return typeof({keyType});");
            }
            Write("throw new ArgumentOutOfRangeException(\"Unknown type: \" + type.ToString());");
            UnIndent(); Write("}");

            Write("public override Type GetValueType(Type type)");
            Write("{"); Indent();
            foreach (var o in classSyntaxReceiver.Classes)
            {
                var classObject = o.Value;
                if (!(classObject.IsList || classObject.IsDictionary || classObject.IsArray))
                {
                    continue;
                }

                string valueType;
                if (classObject.IsDictionary)
                {
                    valueType = ((INamedTypeSymbol)classObject.ModuleSymbol).TypeArguments[1].GetFullName().Replace("?", string.Empty);
                }
                else if (classObject.IsList)
                {
                    valueType = ((INamedTypeSymbol)classObject.ModuleSymbol).TypeArguments[0].GetFullName().Replace("?", string.Empty);
                }
                else
                {
                    valueType = ((IArrayTypeSymbol)(classObject.ModuleSymbol)).ElementType.GetFullName().Replace("?", string.Empty);
                }

                Write($"if (type == typeof({classObject.ModuleSymbol.GetFullName().Replace("?", string.Empty)})) return typeof({valueType});");
            }

            //always support array and list of all types
            foreach (var o in classSyntaxReceiver.Classes)
            {
                var classObject = o.Value;
                Write($"if (type == typeof({classObject.ModuleSymbol.GetFullName().Replace("?", string.Empty)}[])) return typeof({classObject.ModuleSymbol.GetFullName().Replace("?", string.Empty)});");
                Write($"if (type == typeof(System.Collections.Generic.List<{classObject.ModuleSymbol.GetFullName().Replace("?", string.Empty)}>)) return typeof({classObject.ModuleSymbol.GetFullName().Replace("?", string.Empty)});");
            }

            Write("throw new ArgumentOutOfRangeException(\"Unknown type: \" + type.ToString());");
            UnIndent(); Write("}");
            WriteExecuteMethod(classSyntaxReceiver, "ExecuteOnDeserializing", (c) => c.OnDeserializingMethods);
            WriteExecuteMethod(classSyntaxReceiver, "ExecuteOnDeserialized", (c) => c.OnDeserializedMethods);
            WriteExecuteMethod(classSyntaxReceiver, "ExecuteOnSerializing", (c) => c.OnSerializingMethods);
            WriteExecuteMethod(classSyntaxReceiver, "ExecuteOnSerialized", (c) => c.OnSerializedMethods);
            UnIndent(); Write("}");
        }

        private void WriteExecuteMethod(ClassSyntaxReceiver classSyntaxReceiver, string methodName, Func<ClassObject, IEnumerable<IMethodSymbol>> selector)
        {
            Write($"public override void {methodName}(object value)");
            Write("{"); Indent();
            Write("if (value == null) return;");
            Write("var type = value.GetType();");
            foreach (var o in classSyntaxReceiver.Classes)
            {
                var classObject = o.Value;
                var methods = selector(classObject);
                if (methods.Any())
                {
                    var className = classObject.ModuleSymbol.GetFullName().Replace("?", string.Empty);
                    Write($"if (type == typeof({className}))");
                    Write("{"); Indent();
                    foreach (var m in methods)
                    {
                        Write($"(({className})value).{m.Name}();");
                    }
                    Write("return;");
                    UnIndent(); Write("}");
                }
            }
            UnIndent(); Write("}");
        }
    }
}
