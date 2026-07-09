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

using FluentAssertions;
using Xunit;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace YamlDotNet.Test.Serialization
{
    /// <summary>
    /// Verifies that types declaring <c>implicit operator string</c> are serialized as YAML
    /// string scalars rather than as property mappings.
    /// </summary>
    public class ImplicitStringConversionTests
    {
        private sealed class Quantity
        {
            private readonly string _value;
            public Quantity(string value) => _value = value;
            public static implicit operator Quantity(string s) => new(s);
            public static implicit operator string(Quantity q) => q._value;
            public override string ToString() => _value;
        }

        private sealed class ResourceSpec
        {
            public Quantity Memory { get; set; } = new("0");
            public Quantity Cpu { get; set; } = new("0");
        }

        [Fact]
        public void Type_with_implicit_operator_string_serializes_as_scalar()
        {
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var spec = new ResourceSpec { Memory = "512Mi", Cpu = "100m" };
            var yaml = serializer.Serialize(spec);

            yaml.Should().Contain("memory: 512Mi");
            yaml.Should().Contain("cpu: 100m");
            yaml.Should().NotContain("format:");
        }

        [Fact]
        public void Type_with_implicit_operator_string_serializes_as_double_quoted_when_configured()
        {
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithDefaultScalarStyle(ScalarStyle.DoubleQuoted)
                .Build();

            var spec = new ResourceSpec { Memory = "512Mi", Cpu = "100m" };
            var yaml = serializer.Serialize(spec);

            yaml.Should().Contain("memory: \"512Mi\"");
            yaml.Should().Contain("cpu: \"100m\"");
            yaml.Should().NotContain("format:");
        }

        [Fact]
        public void Type_with_implicit_operator_string_round_trips()
        {
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var original = new ResourceSpec { Memory = "256Mi", Cpu = "250m" };
            var yaml = serializer.Serialize(original);
            var restored = deserializer.Deserialize<ResourceSpec>(yaml);

            ((string)restored.Memory).Should().Be("256Mi");
            ((string)restored.Cpu).Should().Be("250m");
        }
    }
}
