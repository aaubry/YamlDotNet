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
using System.IO;
using Xunit;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace YamlDotNet.Test.Serialization
{
    public class RepresentationModelSerializationTests
    {
        [Theory]
        [InlineData("hello", "hello")]
        [InlineData("'hello'", "hello")]
        [InlineData("\"hello\"", "hello")]
        [InlineData("!!int 123", "123")]
        public void ScalarIsSerializable(string yaml, string expectedValue)
        {
            var deserializer = new Deserializer();
            var node = deserializer.Deserialize<YamlScalarNode>(Yaml.ReaderForText(yaml));

            Assert.NotNull(node);
            Assert.Equal(expectedValue, node.Value);

            var serializer = new SerializerBuilder()
                .Build();
            var buffer = new StringWriter();
            serializer.Serialize(buffer, node);
            Assert.Equal(yaml, buffer.ToString().TrimEnd('\r', '\n', '.'));
        }

        [Theory]
        [InlineData("[a]", new[] { "a" })]
        [InlineData("['a']", new[] { "a" })]
        [InlineData("- a\r\n- b", new[] { "a", "b" })]
        [InlineData("!bla [a]", new[] { "a" })]
        public void SequenceIsSerializable(string yaml, string[] expectedValues)
        {
            var deserializer = new Deserializer();
            var node = deserializer.Deserialize<YamlSequenceNode>(Yaml.ReaderForText(yaml));

            Assert.NotNull(node);
            Assert.Equal(expectedValues.Length, node.Children.Count);
            for (var i = 0; i < expectedValues.Length; i++)
            {
                Assert.Equal(expectedValues[i], ((YamlScalarNode)node.Children[i]).Value);
            }

            var serializer = new SerializerBuilder()
                .Build();
            var buffer = new StringWriter();
            serializer.Serialize(buffer, node);
            Assert.Equal(yaml.NormalizeNewLines(), buffer.ToString().TrimEnd('\r', '\n', '.'));
        }

        [Theory]
        [InlineData("{a: b}", new[] { "a", "b" })]
        [InlineData("{'a': \"b\"}", new[] { "a", "b" })]
        [InlineData("a: b\r\nc: d", new[] { "a", "b", "c", "d" })]
        public void MappingIsSerializable(string yaml, string[] expectedKeysAndValues)
        {
            var deserializer = new Deserializer();
            var node = deserializer.Deserialize<YamlMappingNode>(Yaml.ReaderForText(yaml));

            Assert.NotNull(node);
            Assert.Equal(expectedKeysAndValues.Length / 2, node.Children.Count);
            for (var i = 0; i < expectedKeysAndValues.Length; i += 2)
            {
                Assert.Equal(expectedKeysAndValues[i + 1], ((YamlScalarNode)node.Children[expectedKeysAndValues[i]]).Value);
            }

            var serializer = new SerializerBuilder()
                .Build();
            var buffer = new StringWriter();
            serializer.Serialize(buffer, node);
            Assert.Equal(yaml.NormalizeNewLines(), buffer.ToString().TrimEnd('\r', '\n', '.'));
        }
    }

    public class ByteArrayConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            return type == typeof(byte[]);
        }

        public object ReadYaml(IParser parser, Type type)
        {
            var scalar = (YamlDotNet.Core.Events.Scalar)parser.Current;
            var bytes = Convert.FromBase64String(scalar.Value);
            parser.MoveNext();
            return bytes;
        }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            var bytes = (byte[])value;
            emitter.Emit(new YamlDotNet.Core.Events.Scalar(
                null,
                "tag:yaml.org,2002:binary",
                Convert.ToBase64String(bytes),
                ScalarStyle.Plain,
                false,
                false
            ));
        }
    }
}
