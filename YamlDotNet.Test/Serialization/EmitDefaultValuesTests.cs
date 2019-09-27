using System.ComponentModel;
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

        [Fact]
        public void All_default_values_are_omitted_when_DefaultValuesHandling_is_OmitAll()
        {
            // Arrange
            var sut = new SerializerBuilder()
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults)
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
