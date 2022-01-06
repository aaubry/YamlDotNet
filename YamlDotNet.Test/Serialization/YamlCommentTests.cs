using System;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.EventEmitters;

namespace YamlDotNet.Test.Serialization
{
    public class YamlCommentTests
    {
        protected readonly ITestOutputHelper Output;
        public YamlCommentTests(ITestOutputHelper helper)
        {
            Output = helper;
        }

        #region Simple block comments
        [Fact]
        public void SerializationWithBlockComments()
        {
            var person = new Person { Name = "PandaTea", Age = 100 };

            var serializer = new Serializer();
            var result = serializer.Serialize(person);
            Output.WriteLine(result);

            var deserializer = new Deserializer();
            Action action = () => deserializer.Deserialize<Person>(result);
            action.ShouldNotThrow();

            var lines = SplitByLines(result);

            lines.Should().Contain("# The person's name");
            lines.Should().Contain("# The person's age");
        }

        [Fact]
        public void SerializationWithBlockComments_Multiline()
        {
            var person = new Car();

            var serializer = new Serializer();
            var result = serializer.Serialize(person);
            Output.WriteLine(result);

            var deserializer = new Deserializer();
            Action action = () => deserializer.Deserialize<Car>(result);
            action.ShouldNotThrow();

            var lines = SplitByLines(result);

            lines.Should().Contain("# The car's rightful owner");
            lines.Should().Contain("# or:");
            lines.Should().Contain("# This person owns the car");
        }

        [Fact]
        public void SerializationWithBlockComments_NullValue()
        {
            var serializer = new Serializer();
            Action action = () => serializer.Serialize(new NullComment());
            action.ShouldNotThrow();
        }
        #endregion

        #region Indentation of block comments
        [Fact]
        public void SerializationWithBlockComments_IndentedInSequence()
        {
            var person = new Person { Name = "PandaTea", Age = 100 };

            var serializer = new Serializer();
            var result = serializer.Serialize(new Person[] { person });
            Output.WriteLine(result);

            var deserializer = new Deserializer();
            Action action = () => deserializer.Deserialize<Person[]>(result);
            action.ShouldNotThrow();

            var lines = SplitByLines(result);
            var indent = GetIndent(1);

            lines.Should().Contain("- # The person's name");
            lines.Should().Contain(indent + "# The person's age");
        }

        [Fact]
        public void SerializationWithBlockComments_IndentedInBlock()
        {
            var garage = new Garage
            {
                Car = new Car
                {
                    Owner = new Person { Name = "PandaTea", Age = 100 }
                }
            };

            var serializer = new Serializer();
            var result = serializer.Serialize(garage);
            Output.WriteLine(result);

            var deserializer = new Deserializer();
            Action action = () => deserializer.Deserialize<Garage>(result);
            action.ShouldNotThrow();

            var lines = SplitByLines(result);
            var indent1 = GetIndent(1);
            var indent2 = GetIndent(2);

            lines.Should().Contain(indent1 + "# The car's rightful owner");
            lines.Should().Contain(indent1 + "# or:");
            lines.Should().Contain(indent1 + "# This person owns the car");
            lines.Should().Contain(indent2 + "# The person's name");
            lines.Should().Contain(indent2 + "# The person's age");
        }

        [Fact]
        public void SerializationWithBlockComments_IndentedInBlockAndSequence()
        {
            var garage = new Garage
            {
                Car = new Car
                {
                    Passengers = new[]
                    {
                        new Person { Name = "PandaTea", Age = 100 }
                    }
                }
            };

            var serializer = new Serializer();
            var result = serializer.Serialize(garage);
            Output.WriteLine(result);

            var deserializer = new Deserializer();
            Action action = () => deserializer.Deserialize<Garage>(result);
            action.ShouldNotThrow();

            var lines = SplitByLines(result);
            var indent1 = GetIndent(1);
            var indent2 = GetIndent(2);

            lines.Should().Contain(indent1 + "# The car's rightful owner");
            lines.Should().Contain(indent1 + "- # The person's name");
            lines.Should().Contain(indent2 + "# The person's age");
        }
        #endregion

        #region Flow mapping
        [Fact]
        public void SerializationWithBlockComments_IndentedInBlockAndSequence_WithFlowMapping()
        {
            var garage = new Garage
            {
                Car = new Car
                {
                    Owner = new Person { Name = "Paul", Age = 50 },
                    Passengers = new[]
                    {
                        new Person { Name = "PandaTea", Age = 100 }
                    }
                }
            };

            var serializer = new SerializerBuilder()
                .WithEventEmitter(e => new FlowEmitter(e, typeof(Person)))
                .Build();
            var result = serializer.Serialize(garage);
            Output.WriteLine(result);

            var deserializer = new Deserializer();
            Action action = () => deserializer.Deserialize<Garage>(result);
            action.ShouldNotThrow();

            var lines = SplitByLines(result);
            var indent1 = GetIndent(1);

            lines.Should().Contain("# The car parked in the garage");
            lines.Should().Contain(indent1 + "# The car's rightful owner");
            result.Should().NotContain("The person's name", "because the person's properties are inside of a flow map now");
            result.Should().NotContain("The person's age", "because the person's properties are inside of a flow map now");
        }

        /// <summary>
        /// This emits objects of given types as flow mappings
        /// </summary>
        public class FlowEmitter : ChainedEventEmitter
        {
            private readonly Type[] types;

            public FlowEmitter(IEventEmitter nextEmitter, params Type[] types) : base(nextEmitter)
            {
                this.types = types;
            }

            public override void Emit(MappingStartEventInfo eventInfo, IEmitter emitter)
            {
                foreach (var type in types)
                {
                    if (eventInfo.Source.Type == type)
                    {
                        eventInfo.Style = MappingStyle.Flow;
                        break;
                    }
                }
                base.Emit(eventInfo, emitter);
            }
        }
        #endregion

        class Person
        {
            [YamlMember(Description = "The person's name")]
            public string Name { get; set; }
            [YamlMember(Description = "The person's age")]
            public int Age { get; set; }
        }

        class Car
        {
            [YamlMember(Description = "The car's rightful owner\nor:\nThis person owns the car")]
            public Person Owner { get; set; }
            public Person[] Passengers { get; set; }
        }

        class Garage
        {
            [YamlMember(Description = "The car parked in the garage")]
            public Car Car;
        }

        class NullComment
        {
            [YamlMember(Description = null)]
            public int Foo { get; set; }
        }

        private static string[] SplitByLines(string result)
        {
            return result.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        }

        private static string GetIndent(int depth)
        {
            var indentWidth = EmitterSettings.Default.BestIndent;
            var indent = "";
            while (indent.Length < indentWidth * depth)
            {
                indent += " ";
            }

            return indent;
        }
    }
}
