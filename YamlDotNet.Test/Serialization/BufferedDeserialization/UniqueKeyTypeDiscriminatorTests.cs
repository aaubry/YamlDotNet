using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace YamlDotNet.Test.Serialization.BufferedDeserialization
{
    public class UniqueKeyTypeDiscriminatorTests
    {
        [Fact]
        public void UniqueKeyTypeDiscriminator_WithInterfaceBaseType()
        {
            var bufferedDeserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithBufferedNodeDeserializer(options => {
                    options.AddUniqueKeyTypeDiscriminator<ICharacter>(
                        new Dictionary<string, Type>()
                        {
                            { "cheeseSupply", typeof(Mouse) },
                            { "avgDailyMeows", typeof(Cat) }
                        }
                    );
                    },
                    maxDepth: 3,
                    maxLength: 10)
                .Build();

            var characters = bufferedDeserializer.Deserialize<List<ICharacter>>(TomAndJerryYaml);
            characters[0].Should().BeOfType<Mouse>();
            characters[1].Should().BeOfType<Cat>();
        }

                [Fact]
        public void UniqueKeyTypeDiscriminator_WithObjectBaseType()
        {
            var bufferedDeserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithBufferedNodeDeserializer(options => {
                    options.AddUniqueKeyTypeDiscriminator<object>(
                        new Dictionary<string, Type>()
                        {
                            { "cheeseSupply", typeof(Mouse) },
                            { "avgDailyMeows", typeof(Cat) }
                        }
                    );
                    },
                    maxDepth: 3,
                    maxLength: 10)
                .Build();

            var charactersObj = bufferedDeserializer.Deserialize<object>(TomAndJerryYaml);
            var characters = (List<object>)charactersObj;
            characters[0].Should().BeOfType<Mouse>();
            characters[1].Should().BeOfType<Cat>();
        }

        public const string TomAndJerryYaml = @"
- name: Jerry
  cheeseSupply: 5
- name: Tom
  avgDailyMeows: 20.0
";

        public interface ICharacter { }

        public class Mouse : ICharacter
        {
            public string Name { get; set; }
            public int CheeseSupply { get; set; }
        }

        public class Cat : ICharacter
        {
            public string Name { get; set; }
            public float AvgDailyMeows { get; set; }
        }
    }
}
