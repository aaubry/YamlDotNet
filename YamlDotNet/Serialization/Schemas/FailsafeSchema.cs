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

using YamlDotNet.Core;

namespace YamlDotNet.Serialization.Schemas
{
    public sealed class FailsafeSchema
    {
        public static class Tags
        {
            public static readonly TagName Map = new TagName("tag:yaml.org,2002:map");
            public static readonly TagName Seq = new TagName("tag:yaml.org,2002:seq");
            public static readonly TagName Str = new TagName("tag:yaml.org,2002:str");
        }
    }

    public sealed class JsonSchema
    {
        public static class Tags
        {
            public static readonly TagName Null = new TagName("tag:yaml.org,2002:null");
            public static readonly TagName Bool = new TagName("tag:yaml.org,2002:bool");
#pragma warning disable CA1720 // Identifier contains type name
            public static readonly TagName Int = new TagName("tag:yaml.org,2002:int");
            public static readonly TagName Float = new TagName("tag:yaml.org,2002:float");
#pragma warning restore CA1720
        }
    }

    public sealed class CoreSchema
    {
        public static class Tags
        {
        }
    }

    public sealed class DefaultSchema
    {
        public static class Tags
        {
            public static readonly TagName Timestamp = new TagName("tag:yaml.org,2002:timestamp");
        }
    }
}
