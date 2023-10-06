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
    public class StaticTypeResolverFile : File
    {
        public StaticTypeResolverFile(Action<string, bool> write, Action indent, Action unindent, GeneratorExecutionContext context) : base(write, indent, unindent, context)
        {
        }

        public override void Write(ClassSyntaxReceiver classSyntaxReceiver)
        {
            Write($"class StaticTypeResolver : YamlDotNet.Serialization.TypeResolvers.StaticTypeResolver");
            Write("{"); Indent();
            Write("private readonly YamlDotNet.Serialization.StaticContext _context;");
            Write($"public StaticTypeResolver(YamlDotNet.Serialization.StaticContext context)");
            Write("{"); Indent();
            Write("_context = context;");
            UnIndent(); Write("}");
            Write("public override Type Resolve(Type staticType, object actualValue)");
            Write("{"); Indent();
            Write("var result = base.Resolve(staticType, actualValue);");
            Write("if (result == staticType && actualValue != null && _context.IsKnownType(actualValue.GetType())) result = actualValue.GetType();");
            Write("return result;");
            UnIndent(); Write("}");
            UnIndent(); Write("}");
        }
    }
}
