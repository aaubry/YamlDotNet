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

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Xunit;
using YamlDotNet.Serialization;

namespace YamlDotNet.Test.Serialization
{
    public class EmitDefaultValuesTests
    {
        private class Model
        {
            public string ANullString => null;
            [DefaultValue("hello")] public string ADefaultString => "hello";
            [DefaultValue("hello")] public string ANonDefaultString => "world";
            [DefaultValue("hello")] public string ANonDefaultNullString => null;

            public int AZeroInteger => 0;
            public int ANonZeroInteger => 1;
            [DefaultValue(2)] public int ADefaultInteger => 2;
            [DefaultValue(2)] public int ANonDefaultZeroInteger => 0;

            public int? ANullInteger => null;
            public int? ANullableZeroInteger => 0;
            public int? ANullableNonZeroInteger => 1;
            [DefaultValue(2)] public int? ANullableNonZeroDefaultInteger => 2;
            [DefaultValue(2)] public int? ANullableNonZeroNonDefaultInteger => 1;

            // Enumerables
            public IList<int> ANullList => null;

            public int[] AnEmptyArray => new int[0];
            public IList<int> AnEmptyList => new List<int>();
            public Dictionary<string, string> AnEmptyDictionary => new Dictionary<string, string>();
            public IEnumerable<int> AnEmptyEnumerable => Enumerable.Empty<int>();

            public string[] AnNonEmptyArray => new[] { "foo", "bar" };
            public IList<int> AnNonEmptyList => new List<int> { 6, 9, 42 };
            public IEnumerable<bool> ANonEmptyEnumerable => new[] { true, false };
            public Dictionary<string, string> ANonEmptyDictionary => new Dictionary<string, string>() { { "foo", "bar" } };

        }

        [Fact]
        public void All_default_values_and_nulls_are_emitted_when_no_configuration_is_performed()
        {
            // Arrange
            var sut = new SerializerBuilder()
                .Build();

            // Act
            var yaml = sut.Serialize(new Model());

            // Assert
            Assert.Contains(nameof(Model.ANullString) + ':', yaml);
            Assert.Contains(nameof(Model.ADefaultString) + ':', yaml);
            Assert.Contains(nameof(Model.ANonDefaultString) + ':', yaml);
            Assert.Contains(nameof(Model.ANonDefaultNullString) + ':', yaml);

            Assert.Contains(nameof(Model.AZeroInteger) + ':', yaml);
            Assert.Contains(nameof(Model.ANonZeroInteger) + ':', yaml);
            Assert.Contains(nameof(Model.ADefaultInteger) + ':', yaml);
            Assert.Contains(nameof(Model.ANonDefaultZeroInteger) + ':', yaml);

            Assert.Contains(nameof(Model.ANullInteger) + ':', yaml);
            Assert.Contains(nameof(Model.ANullableZeroInteger) + ':', yaml);
            Assert.Contains(nameof(Model.ANullableNonZeroInteger) + ':', yaml);
            Assert.Contains(nameof(Model.ANullableNonZeroDefaultInteger) + ':', yaml);
            Assert.Contains(nameof(Model.ANullableNonZeroNonDefaultInteger) + ':', yaml);
        }

        [Fact]
        public void Only_null_values_are_omitted_when_DefaultValuesHandling_is_OmitNull()
        {
            // Arrange
            var sut = new SerializerBuilder()
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
                .Build();

            // Act
            var yaml = sut.Serialize(new Model());

            // Assert
            Assert.DoesNotContain(nameof(Model.ANullString) + ':', yaml);
            Assert.Contains(nameof(Model.ADefaultString) + ':', yaml);
            Assert.Contains(nameof(Model.ANonDefaultString) + ':', yaml);
            Assert.DoesNotContain(nameof(Model.ANonDefaultNullString) + ':', yaml);

            Assert.Contains(nameof(Model.AZeroInteger) + ':', yaml);
            Assert.Contains(nameof(Model.ANonZeroInteger) + ':', yaml);
            Assert.Contains(nameof(Model.ADefaultInteger) + ':', yaml);
            Assert.Contains(nameof(Model.ANonDefaultZeroInteger) + ':', yaml);

            Assert.DoesNotContain(nameof(Model.ANullInteger) + ':', yaml);
            Assert.Contains(nameof(Model.ANullableZeroInteger) + ':', yaml);
            Assert.Contains(nameof(Model.ANullableNonZeroInteger) + ':', yaml);
            Assert.Contains(nameof(Model.ANullableNonZeroDefaultInteger) + ':', yaml);
            Assert.Contains(nameof(Model.ANullableNonZeroNonDefaultInteger) + ':', yaml);
        }

        [Theory]
        [InlineData(DefaultValuesHandling.OmitDefaults)]
        [InlineData(DefaultValuesHandling.OmitDefaultsOrEmpty)]
        public void All_default_values_are_omitted_when_DefaultValuesHandling_is_OmitDefaults(DefaultValuesHandling defaultValuesHandling)
        {
            // Arrange
            var sut = new SerializerBuilder()
                .ConfigureDefaultValuesHandling(defaultValuesHandling)
                .Build();

            // Act
            var yaml = sut.Serialize(new Model());

            // Assert
            Assert.DoesNotContain(nameof(Model.ANullString) + ':', yaml);
            Assert.DoesNotContain(nameof(Model.ADefaultString) + ':', yaml);
            Assert.Contains(nameof(Model.ANonDefaultString) + ':', yaml);
            Assert.Contains(nameof(Model.ANonDefaultNullString) + ':', yaml);

            Assert.DoesNotContain(nameof(Model.AZeroInteger) + ':', yaml);
            Assert.Contains(nameof(Model.ANonZeroInteger) + ':', yaml);
            Assert.DoesNotContain(nameof(Model.ADefaultInteger) + ':', yaml);
            Assert.Contains(nameof(Model.ANonDefaultZeroInteger) + ':', yaml);

            Assert.DoesNotContain(nameof(Model.ANullInteger) + ':', yaml);
            Assert.Contains(nameof(Model.ANullableZeroInteger) + ':', yaml);
            Assert.Contains(nameof(Model.ANullableNonZeroInteger) + ':', yaml);
            Assert.DoesNotContain(nameof(Model.ANullableNonZeroDefaultInteger) + ':', yaml);
            Assert.Contains(nameof(Model.ANullableNonZeroNonDefaultInteger) + ':', yaml);
        }

        [Fact]
        public void Empty_enumerables_are_omitted_when_DefaultValuesHandling_is_OmitDefaultsOrEmpty()
        {
            // Arrange
            var sut = new SerializerBuilder()
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaultsOrEmpty)
                .Build();

            // Act
            var yaml = sut.Serialize(new Model());

            // Assert defaults
            Assert.DoesNotContain(nameof(Model.ANullString) + ':', yaml);
            Assert.DoesNotContain(nameof(Model.ADefaultString) + ':', yaml);
            Assert.Contains(nameof(Model.ANonDefaultString) + ':', yaml);
            Assert.Contains(nameof(Model.ANonDefaultNullString) + ':', yaml);

            Assert.DoesNotContain(nameof(Model.AZeroInteger) + ':', yaml);
            Assert.Contains(nameof(Model.ANonZeroInteger) + ':', yaml);
            Assert.DoesNotContain(nameof(Model.ADefaultInteger) + ':', yaml);
            Assert.Contains(nameof(Model.ANonDefaultZeroInteger) + ':', yaml);

            Assert.DoesNotContain(nameof(Model.ANullInteger) + ':', yaml);
            Assert.Contains(nameof(Model.ANullableZeroInteger) + ':', yaml);
            Assert.Contains(nameof(Model.ANullableNonZeroInteger) + ':', yaml);
            Assert.DoesNotContain(nameof(Model.ANullableNonZeroDefaultInteger) + ':', yaml);
            Assert.Contains(nameof(Model.ANullableNonZeroNonDefaultInteger) + ':', yaml);

            // Assert enumerables
            Assert.DoesNotContain(nameof(Model.ANullList) + ':', yaml);

            Assert.DoesNotContain(nameof(Model.AnEmptyArray) + ':', yaml);
            Assert.DoesNotContain(nameof(Model.AnEmptyList) + ':', yaml);
            Assert.DoesNotContain(nameof(Model.AnEmptyDictionary) + ':', yaml);
            Assert.DoesNotContain(nameof(Model.AnEmptyEnumerable) + ':', yaml);

            Assert.Contains(nameof(Model.AnNonEmptyArray) + ':', yaml);
            Assert.Contains(nameof(Model.AnNonEmptyList) + ':', yaml);
            Assert.Contains(nameof(Model.ANonEmptyEnumerable) + ':', yaml);
            Assert.Contains(nameof(Model.ANonEmptyDictionary) + ':', yaml);
        }

        [Fact]
        public void YamlMember_overrides_default_value_handling()
        {
            // Arrange
            var sut = new SerializerBuilder()
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
                .WithAttributeOverride<Model>(m => m.ANullString, new YamlMemberAttribute { DefaultValuesHandling = DefaultValuesHandling.Preserve })
                .Build();

            // Act
            var yaml = sut.Serialize(new Model());

            // Assert
            Assert.Contains(nameof(Model.ANullString) + ':', yaml);
            Assert.DoesNotContain(nameof(Model.ANullInteger) + ':', yaml);
        }
    }
}
