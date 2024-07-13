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
using System.Text;
using System.Threading.Tasks;
using Xunit;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace YamlDotNet.Test.Serialization
{
    public class TypeConverterAttributeTests
    {
        [Fact]
        public void TestConverterOnAttribute_Deserializes()
        {
            var deserializer = new DeserializerBuilder().WithTypeConverter(new AttributedTypeConverter()).Build();
            var yaml = @"Value:
  abc: def";
            var actual = deserializer.Deserialize<OuterClass>(yaml);
            Assert.Equal("abc", actual.Value.Key);
            Assert.Equal("def", actual.Value.Value);
        }

        [Fact]
        public void TestConverterOnAttribute_Serializes()
        {
            var serializer = new SerializerBuilder().WithTypeConverter(new AttributedTypeConverter()).Build();
            var o = new OuterClass
            {
                Value = new ValueClass
                {
                    Key = "abc",
                    Value = "def"
                }
            };
            var actual = serializer.Serialize(o).NormalizeNewLines().TrimNewLines();
            var expected = @"Value:
  abc: def";
            Assert.Equal(expected, actual);
        }

        public class AttributedTypeConverter : IYamlTypeConverter
        {
            public bool Accepts(Type type) => false;

            public object ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
            {
                parser.Consume<MappingStart>();
                var key = parser.Consume<Scalar>();
                var value = parser.Consume<Scalar>();
                parser.Consume<MappingEnd>();

                var result = new ValueClass
                {
                    Key = key.Value,
                    Value = value.Value
                };
                return result;
            }

            public void WriteYaml(IEmitter emitter, object value, Type type, ObjectSerializer serializer)
            {
                var v = (ValueClass)value;

                emitter.Emit(new MappingStart());
                emitter.Emit(new Scalar(v.Key));
                emitter.Emit(new Scalar(v.Value));
                emitter.Emit(new MappingEnd());
            }
        }

        public class OuterClass
        {
            [YamlConverter(typeof(AttributedTypeConverter))]
            public ValueClass Value { get; set; }
        }

        public class ValueClass
        {
            public string Key { get; set; }
            public string Value { get; set; }
        }
    }
}
