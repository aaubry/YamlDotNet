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


#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using FluentAssertions;
using Xunit;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.TypeInspectors;

namespace YamlDotNet.Test.Serialization
{
    public class DataMemberTypeInspectorTests
    {
        public class Sample
        {
            [DataMember(Name = "renamed")]
            public string Original { get; set; } = "a";

            [IgnoreDataMember]
            public string Hidden { get; set; } = "secret";

            public string Plain { get; set; } = "p";
        }

        public class Ordered
        {
            [DataMember(Order = 2)]
            public string Second { get; set; } = "2";

            [DataMember(Order = 1)]
            public string First { get; set; } = "1";
        }

        public class ExplicitZero
        {
            public string Plain { get; set; } = "p";

            [DataMember(Order = 0)]
            public string Zero { get; set; } = "z";
        }

        public class Unset
        {
            [DataMember]
            public string B { get; set; } = "b";

            [DataMember]
            public string A { get; set; } = "a";

            public string C { get; set; } = "c";
        }

        public class Both
        {
            [YamlMember(Alias = "yaml")]
            [DataMember(Name = "data")]
            public string Value { get; set; } = "v";
        }

        private static ISerializer CreateSerializer() => new SerializerBuilder()
            .WithTypeInspector(inner => new DataMemberTypeInspector(inner))
            .Build();

        private static IDeserializer CreateDeserializer() => new DeserializerBuilder()
            .WithTypeInspector(inner => new DataMemberTypeInspector(inner))
            .Build();

        [Fact]
        public void SerializeHonorsNameAndIgnore()
        {
            var yaml = CreateSerializer().Serialize(new Sample());
            yaml.Should().Contain("renamed: a").And.Contain("Plain: p").And.NotContain("Hidden").And.NotContain("Original");
        }

        [Fact]
        public void SerializeHonorsOrder()
        {
            var yaml = CreateSerializer().Serialize(new Ordered());
            yaml.IndexOf("First").Should().BeLessThan(yaml.IndexOf("Second"));
        }

        [Fact]
        public void DeserializeHonorsName()
        {
            var result = CreateDeserializer().Deserialize<Sample>("renamed: x\nPlain: y\n");
            result.Original.Should().Be("x");
            result.Plain.Should().Be("y");
        }

        [Fact]
        public void DeserializeIgnoresIgnoredMember()
        {
            var act = () => CreateDeserializer().Deserialize<Sample>("Hidden: z\n");
            act.Should().Throw<YamlDotNet.Core.YamlException>();
        }

        [Fact]
        public void AttributesAreIgnoredByDefault()
        {
            var yaml = new SerializerBuilder().Build().Serialize(new Sample());
            yaml.Should().Contain("Original: a").And.Contain("Hidden: secret");
        }

        [Fact]
        public void AttributeDefaultOrderIsMinusOne()
        {
            var member = typeof(Unset).GetProperty(nameof(Unset.A))!.GetCustomAttributes(typeof(DataMemberAttribute), false);
            ((DataMemberAttribute)member[0]).Order.Should().Be(-1);
        }

        [Fact]
        public void UnsetOrderPreservesDeclarationOrder()
        {
            var yaml = CreateSerializer().Serialize(new Unset());
            yaml.Should().Be("B: b\nA: a\nC: c\n".Replace("\n", System.Environment.NewLine));
        }

        [Fact]
        public void ExplicitZeroOrderIsHonored()
        {
            // Plain keeps the inner order (0) and is declared first, so Zero follows it; both sort stably.
            var yaml = CreateSerializer().Serialize(new ExplicitZero());
            yaml.IndexOf("Plain").Should().BeLessThan(yaml.IndexOf("Zero"));
        }

        [Fact]
        public void RoundTripPreservesValues()
        {
            var yaml = CreateSerializer().Serialize(new Sample { Original = "x", Plain = "y" });
            var result = CreateDeserializer().Deserialize<Sample>(yaml);
            result.Original.Should().Be("x");
            result.Plain.Should().Be("y");
            result.Hidden.Should().Be("secret");
        }

        [Fact]
        public void WhenStackedOnTopDataMemberNameWinsOverYamlMemberAlias()
        {
            var yaml = CreateSerializer().Serialize(new Both());
            yaml.Should().Contain("data: v").And.NotContain("yaml:");
        }

        [Fact]
        public void WhenRegisteredAfterYamlAttributesYamlMemberAliasWins()
        {
            var yaml = new SerializerBuilder()
                .WithTypeInspector(inner => new DataMemberTypeInspector(inner), loc => loc.After<YamlAttributesTypeInspector>())
                .Build()
                .Serialize(new Both());
            yaml.Should().Contain("yaml: v").And.NotContain("data:");
        }

        public class Decorated
        {
            [DataMember(Order = 0)]
            public string ExplicitZero { get; set; } = string.Empty;

            [DataMember(Name = "named")]
            public string Unset { get; set; } = string.Empty;

            public string Unattributed { get; set; } = string.Empty;
        }

        private sealed class StubDescriptor : IPropertyDescriptor
        {
            private readonly System.Reflection.PropertyInfo property;

            public StubDescriptor(System.Reflection.PropertyInfo property, int order)
            {
                this.property = property;
                Order = order;
            }

            public string Name => property.Name;
            public bool AllowNulls => true;
            public bool CanWrite => true;
            public Type Type => property.PropertyType;
            public Type? TypeOverride { get; set; }
            public int Order { get; set; }
            public ScalarStyle ScalarStyle { get; set; }
            public bool Required => false;
            public Type? ConverterType => null;

            public T? GetCustomAttribute<T>() where T : Attribute
                => (T?)Attribute.GetCustomAttribute(property, typeof(T));

            public IObjectDescriptor Read(object target) => throw new NotSupportedException();
            public void Write(object target, object? value) => throw new NotSupportedException();
        }

        private sealed class StubInspector : TypeInspectorSkeleton
        {
            public override string GetEnumName(Type enumType, string name) => name;
            public override string GetEnumValue(object enumValue) => enumValue.ToString()!;
            public override bool HasParseMethod(Type type) => false;
            public override object? Parse(string value, Type expectedType) => null;

            public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object? container)
                => type.GetProperties().Select(p => (IPropertyDescriptor)new StubDescriptor(p, 7));
        }

        private static Dictionary<string, IPropertyDescriptor> InspectDecorated()
            => new DataMemberTypeInspector(new StubInspector())
                .GetProperties(typeof(Decorated), null)
                .ToDictionary(p => p.Name);

        [Fact]
        public void ExplicitZeroOrderOverridesNonZeroInnerOrder()
        {
            InspectDecorated()[nameof(Decorated.ExplicitZero)].Order.Should().Be(0);
        }

        [Fact]
        public void UnsetOrderPreservesNonZeroInnerOrderWhileRenaming()
        {
            var props = InspectDecorated();
            props.Should().ContainKey("named");
            props["named"].Order.Should().Be(7);
        }

        [Fact]
        public void UnattributedDescriptorIsReturnedUnchanged()
        {
            var props = InspectDecorated();
            props[nameof(Decorated.Unattributed)].Should().BeOfType<StubDescriptor>();
            props[nameof(Decorated.Unattributed)].Order.Should().Be(7);
        }

        [Fact]
        public void ExplicitZeroSortsBeforeInnerOrderSeven()
        {
            new DataMemberTypeInspector(new StubInspector())
                .GetProperties(typeof(Decorated), null)
                .First().Name.Should().Be(nameof(Decorated.ExplicitZero));
        }
    }
}
