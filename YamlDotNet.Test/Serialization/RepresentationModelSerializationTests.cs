using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            for (int i = 0; i < expectedValues.Length; i++)
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
            for (int i = 0; i < expectedKeysAndValues.Length; i += 2)
            {
                YamlNode key = expectedKeysAndValues[i];
                Assert.Equal(expectedKeysAndValues[i + 1], ((YamlScalarNode)node.Children[key]).Value);
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
                AnchorName.Empty,
                "tag:yaml.org,2002:binary",
                Convert.ToBase64String(bytes),
                ScalarStyle.Plain));
        }
    }
}
