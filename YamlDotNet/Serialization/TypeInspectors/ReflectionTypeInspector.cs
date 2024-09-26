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
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace YamlDotNet.Serialization.TypeInspectors
{
    public abstract class ReflectionTypeInspector : TypeInspectorSkeleton
    {
        public override string GetEnumName(Type enumType, string name)
        {
#if NETSTANDARD2_0_OR_GREATER || NET6_0_OR_GREATER
            foreach (var enumMember in enumType.GetMembers())
            {
                var attribute = enumMember.GetCustomAttribute<EnumMemberAttribute>();
                if (attribute?.Value == name)
                {
                    return enumMember.Name;
                }
            }
#endif
            return name;
        }
        public override string GetEnumValue(object enumValue)
        {
            if (enumValue == null)
            {
                return string.Empty;
            }

            var result = enumValue.ToString();
#if NETSTANDARD2_0_OR_GREATER || NET6_0_OR_GREATER
            var type = enumValue.GetType();
            var enumMembers = type.GetMember(result);
            if (enumMembers.Length > 0)
            {
                var attribute = enumMembers[0].GetCustomAttribute<EnumMemberAttribute>();
                if (attribute?.Value != null)
                {
                    result = attribute.Value;
                }
            }
#endif
            return result!;
        }

    }
}
