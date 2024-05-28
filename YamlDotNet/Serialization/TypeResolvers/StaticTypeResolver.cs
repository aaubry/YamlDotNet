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

namespace YamlDotNet.Serialization.TypeResolvers
{
    /// <summary>
    /// Except for primitive types, the type returned will always be the static type.
    /// </summary>
    public class StaticTypeResolver : ITypeResolver
    {
        public virtual Type Resolve(Type staticType, object? actualValue)
        {
            if (actualValue != null)
            {
                if (actualValue.GetType().IsEnum)
                {
                    return staticType;
                }

                switch (actualValue.GetType().GetTypeCode())
                {
                    case TypeCode.Boolean: return typeof(bool);
                    case TypeCode.Char: return typeof(char);
                    case TypeCode.SByte: return typeof(sbyte);
                    case TypeCode.Byte: return typeof(byte);
                    case TypeCode.Int16: return typeof(short);
                    case TypeCode.UInt16: return typeof(ushort);
                    case TypeCode.Int32: return typeof(int);
                    case TypeCode.UInt32: return typeof(uint);
                    case TypeCode.Int64: return typeof(long);
                    case TypeCode.UInt64: return typeof(ulong);
                    case TypeCode.Single: return typeof(float);
                    case TypeCode.Double: return typeof(double);
                    case TypeCode.Decimal: return typeof(decimal);
                    case TypeCode.String: return typeof(string);
                    case TypeCode.DateTime: return typeof(DateTime);
                }
            }

            return staticType;
        }
    }
}
