// This file is part of YamlDotNet - A .NET library for YAML.
// Copyright (c) Antoine Aubry
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using YamlDotNet.Core.Events;
using System.Text.RegularExpressions;

namespace YamlDotNet.Serialization.NodeTypeResolvers
{
    public sealed class TagNodeTypeResolver : INodeTypeResolver
    {
        private readonly IDictionary<string, Type> tagMappings;

        public TagNodeTypeResolver(IDictionary<string, Type> tagMappings)
        {
            this.tagMappings = tagMappings ?? throw new ArgumentNullException(nameof(tagMappings));
        }

        private static List<(Regex, string)> ImplicitRegEx { get; } = new List<(Regex, string)>();

        static TagNodeTypeResolver()
        {
            //see 10.3.2 Tag Resolution (https://yaml.org/spec/1.2/spec.html#id2804356)
            void AddRegExp(string pattern, string tag, string tagPrefix = "tag:yaml.org,2002:")
            {
                ImplicitRegEx.Add((new Regex(pattern, RegexOptions.Compiled), $"{tagPrefix}{tag}"));
            }

            //Null and bool are handled special (can be ignored here)
            AddRegExp("^[-+]?[0-9]+$", "int");
            AddRegExp("^0o[0-7]+$", "int");
            AddRegExp("^0x[0-9a-fA-F]+$", "int");
            AddRegExp(@"^[-+]?(\.[0-9]+|[0-9]+(\.[0-9]*)?)([eE][-+]?[0-9]+)?$", "float");
            AddRegExp(@"^[-+]?(\.inf|\.Inf|\.INF)$", "float");
            AddRegExp(@"^[-+]?(\.nan|\.NaN|\.NAN)$", "float");
        }

        private bool ResolveImplicitTag(NodeEvent nodeEvent, ref string tag)
        {
            string value;
            if (nodeEvent is Scalar scalar && scalar.Style == Core.ScalarStyle.Plain)
            {
                value = scalar.Value;
                foreach(var (regEx, t) in ImplicitRegEx)
                {
                    if (regEx.IsMatch(value))
                    {
                        tag = t;
                        return true;
                    }
                }
            }
            return false;
        }

        bool INodeTypeResolver.Resolve(NodeEvent? nodeEvent, ref Type currentType)
        {
            if (nodeEvent != null)
            {
                var tag = nodeEvent.Tag;
                if (string.IsNullOrEmpty(tag))
                {
                    if (!ResolveImplicitTag(nodeEvent, ref tag))
                    {
                        return false;
                    }
                }
                if (tagMappings.TryGetValue(tag, out var predefinedType))
                {
                    currentType = predefinedType;
                    return true;
                }
            }
            return false;
        }
    }
}
