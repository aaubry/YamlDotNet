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
using Xunit;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.Converters;

namespace YamlDotNet.Test.Serialization
{
    public class DateTime8601ConverterTests
    {
        [Fact]
        public void Uses8601TimeFormat_UTC()
        {
            var serializer = new SerializerBuilder()
                .WithTypeConverter(new DateTime8601Converter(ScalarStyle.Plain))
                .Build();
            var actual = serializer.Serialize(new DateTime(2024, 07, 11, 18, 05, 07, DateTimeKind.Utc)).TrimNewLines();
            Assert.Equal("2024-07-11T18:05:07.0000000Z", actual);
        }

        [Fact]
        public void Uses8601TimeFormat_Unspecified()
        {
            var serializer = new SerializerBuilder()
                .WithTypeConverter(new DateTime8601Converter(ScalarStyle.Plain))
                .Build();
            var actual = serializer.Serialize(new DateTime(2024, 07, 11, 18, 05, 07)).TrimNewLines();
            Assert.Equal("2024-07-11T18:05:07.0000000", actual);
        }

        [Fact]
        public void Uses8601TimeFormat_Local()
        {
            var serializer = new SerializerBuilder()
                .WithTypeConverter(new DateTime8601Converter(ScalarStyle.DoubleQuoted))
                .Build();
            var dt = new DateTime(2024, 07, 11, 18, 05, 07, DateTimeKind.Local);
            var actual = serializer.Serialize(dt).TrimNewLines();

            Assert.Equal($"\"{dt.ToString("O")}\"", actual);
        }
    }
}
