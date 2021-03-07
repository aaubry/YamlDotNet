//  This file is part of YamlDotNet - A .NET library for YAML.
//  Copyright (c) Antoine Aubry and contributors

//  Permission is hereby granted, free of charge, to any person obtaining a copy of
//  this software and associated documentation files (the "Software"), to deal in
//  the Software without restriction, including without limitation the rights to
//  use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
//  of the Software, and to permit persons to whom the Software is furnished to do
//  so, subject to the following conditions:

//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.

//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YamlDotNet.Core;

namespace YamlDotNet.Serialization.Schemas
{
    public sealed class TypeNameTagNameResolver : ITagNameResolver
    {
        public static readonly ITagNameResolver Instance = new TypeNameTagNameResolver();

        private TypeNameTagNameResolver() { }

        public bool Resolve(Type type, out TagName tag)
        {
            var typeName = new StringBuilder(1024);
            WriteTypeName(type, typeName);
            tag = "!dotnet:" + Uri.EscapeUriString(typeName.ToString());
            return true;
        }

        private static readonly IList<Type> EmptyTypes = new Type[0];

        private static void WriteTypeName(Type type, StringBuilder text)
        {
            var genericArguments = type.IsGenericType()
                ? type.GetGenericArguments()
                : EmptyTypes;

            if (type.IsGenericParameter)
            {
            }
            else if (type.IsNested)
            {
                var parentType = type.DeclaringType!;
                if (parentType.IsGenericTypeDefinition())
                {
                    var nestedTypeArguments = genericArguments
                        .Zip(type.GetGenericTypeDefinition().GetGenericArguments(), (concrete, generic) => new { name = generic.Name, type = concrete });

                    genericArguments = new List<Type>();
                    var parentTypeArguments = parentType.GetGenericArguments();

                    foreach (var childTypeArgument in nestedTypeArguments)
                    {
                        var belongsToParent = false;
                        for (int i = 0; i < parentTypeArguments.Length; ++i)
                        {
                            if (parentTypeArguments[i].Name == childTypeArgument.name)
                            {
                                belongsToParent = true;
                                parentTypeArguments[i] = childTypeArgument.type;
                                break;
                            }
                        }
                        if (!belongsToParent)
                        {
                            genericArguments.Add(childTypeArgument.type);
                        }
                    }

                    if (!type.IsGenericTypeDefinition())
                    {
                        parentType = parentType.MakeGenericType(parentTypeArguments);
                    }
                }

                WriteTypeName(parentType, text);
                text.Append('.');
            }
            else if (!string.IsNullOrEmpty(type.Namespace))
            {
                text.Append(type.Namespace).Append('.');
            }

            var name = type.Name;
            if (type.IsGenericType())
            {
                text.Append(name);
                var quoteIndex = name.IndexOf('`');
                if (name.IndexOf('`') >= 0)
                {
                    text.Length -= name.Length - quoteIndex; // Remove the "`1"
                }

                if (genericArguments.Count > 0)
                {
                    text.Append('(');
                    foreach (var arg in genericArguments)
                    {
                        WriteTypeName(arg, text);
                        text.Append(", ");
                    }
                    text.Length -= 2; // Remove the last ", "
                    text.Append(')');
                }
            }
            else
            {
                text.Append(name);
            }
        }
    }
}
