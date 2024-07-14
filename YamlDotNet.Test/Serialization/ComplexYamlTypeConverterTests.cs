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
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace YamlDotNet.Test.Serialization
{
    public class ComplexYamlTypeConverterTests
    {
        [Fact]
        public void ComplexTypeConverter_UsesSerializerToSerializeComplexTypes()
        {
            var serializer = new SerializerBuilder().WithTypeConverter(new ComplexTypeConverter()).Build();
            var o = new ComplexType
            {
                InnerType1 = new InnerType
                {
                    Prop1 = "prop1",
                    Prop2 = "prop2"
                },
                InnerType2 = new InnerType
                {
                    Prop1 = "2.1",
                    Prop2 = "2.2"
                }
            };
            var actual = serializer.Serialize(o);
            var expected = @"inner.prop1: prop1
inner.prop2: prop2
prop2:
  Prop1: 2.1
  Prop2: 2.2".NormalizeNewLines();
            Assert.Equal(expected, actual.NormalizeNewLines().TrimNewLines());
        }

        [Fact]
        public void ComplexTypeConverter_UsesDeserializerToDeserializeComplexTypes()
        {
            var deserializer = new DeserializerBuilder().WithTypeConverter(new ComplexTypeConverter()).Build();
            var yaml = @"inner.prop1: prop1
inner.prop2: prop2
prop2:
  Prop1: 2.1
  Prop2: 2.2";
            var actual = deserializer.Deserialize<ComplexType>(yaml);
            Assert.Equal("prop1", actual.InnerType1.Prop1);
            Assert.Equal("prop2", actual.InnerType1.Prop2);
            Assert.Equal("2.1", actual.InnerType2.Prop1);
            Assert.Equal("2.2", actual.InnerType2.Prop2);
        }

        private class ComplexType
        {
            public InnerType InnerType1 { get; set; }
            public InnerType InnerType2 { get; set; }
        }

        private class InnerType
        {
            public string Prop1 { get; set; }
            public string Prop2 { get; set; }
        }

        private class ComplexTypeConverter : IYamlTypeConverter
        {
            public bool Accepts(Type type)
            {
                if (type == typeof(ComplexType))
                {
                    return true;
                }
                return false;
            }

            public object ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
            {
                parser.Consume<MappingStart>();

                var result = new ComplexType();
                result.InnerType1 = new InnerType();

                Consume(parser, result, rootDeserializer);
                Consume(parser, result, rootDeserializer);
                Consume(parser, result, rootDeserializer);

                parser.Consume<MappingEnd>();

                return result;
            }

            public void WriteYaml(IEmitter emitter, object value, Type type, ObjectSerializer serializer)
            {
                var c = (ComplexType)value;
                emitter.Emit(new MappingStart());
                emitter.Emit(new Scalar("inner.prop1"));
                emitter.Emit(new Scalar(c.InnerType1.Prop1));
                emitter.Emit(new Scalar("inner.prop2"));
                emitter.Emit(new Scalar(c.InnerType1.Prop2));
                emitter.Emit(new Scalar("prop2"));
                serializer(c.InnerType2);
                emitter.Emit(new MappingEnd());
            }

            private void Consume(IParser parser, ComplexType type, ObjectDeserializer deserializer)
            {
                var name = parser.Consume<Scalar>();
                if (name.Value == "inner.prop1")
                {
                    var value = parser.Consume<Scalar>();
                    type.InnerType1.Prop1 = value.Value;
                }
                else if (name.Value == "inner.prop2")
                {
                    var value = parser.Consume<Scalar>();
                    type.InnerType1.Prop2 = value.Value;
                }
                else if (name.Value == "prop2")
                {
                    var value = deserializer(typeof(InnerType));
                    type.InnerType2 = (InnerType)value;
                }
                else
                {
                    throw new Exception("Invalid property name");
                }
            }
        }
    }
}
