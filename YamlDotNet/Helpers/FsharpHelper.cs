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
using YamlDotNet.Serialization;

namespace YamlDotNet.Helpers
{
    public static class FsharpHelper
    {
        private static bool IsFsharpCore(Type t)
        {
            return t.Namespace == "Microsoft.FSharp.Core";
        }

        public static bool IsOptionType(Type t)
        {
            return IsFsharpCore(t) && t.Name == "FSharpOption`1";
        }

        public static Type? GetOptionUnderlyingType(Type t)
        {
            return t.IsGenericType && IsOptionType(t) ? t.GenericTypeArguments[0] : null;
        }

        public static object? GetValue(IObjectDescriptor objectDescriptor)
        {
            if (!IsOptionType(objectDescriptor.Type))
            {
                throw new InvalidOperationException("Should not be called on non-Option<> type");
            }

            if (objectDescriptor.Value is null)
            {
                return null;
            }

            return objectDescriptor.Type.GetProperty("Value").GetValue(objectDescriptor.Value);
        }

        public static bool IsFsharpListType(Type t)
        {
            return t.Namespace == "Microsoft.FSharp.Collections" && t.Name == "FSharpList`1";
        }

        public static object? CreateFsharpListFromArray(Type t, Type itemsType, Array arr)
        {
            if (!IsFsharpListType(t))
            {
                return null;
            }

            var fsharpList =
                t.Assembly
                .GetType("Microsoft.FSharp.Collections.ListModule")
                .GetMethod("OfArray")
                .MakeGenericMethod(itemsType)
                .Invoke(null, new []{ arr });

            return fsharpList;
        }
    }
}
