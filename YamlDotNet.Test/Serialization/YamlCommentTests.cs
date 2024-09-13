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
            var multilineComment = new MultilineComment();

            var serializer = new Serializer();
            var result = serializer.Serialize(multilineComment);
            Output.WriteLine(result);

            var deserializer = new Deserializer();
            Action action = () => deserializer.Deserialize<MultilineComment>(result);
            action.ShouldNotThrow();

            var lines = SplitByLines(result);

            lines[0].Should().Be("# This");
            lines[1].Should().Be("# is");
            lines[2].Should().Be("# multiline");

            lines[4].Should().Be("# This is");
            lines[5].Should().Be("# too");
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
            [YamlMember(Description = "The car's rightful owner")]
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

        class MultilineComment
        {
            [YamlMember(Description = "This\nis\nmultiline")]
            public int Foo { get; set; }

            [YamlMember(Description = @"This is
too")]
            public int Bar { get; set; }
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
