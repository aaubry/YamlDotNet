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
using Xunit.Abstractions;
using YamlDotNet.Serialization;

namespace YamlDotNet.Test.Serialization
{
    public class YamlNewLineTests
    {
        protected readonly ITestOutputHelper Output;

        public YamlNewLineTests(ITestOutputHelper helper)
        {
            Output = helper;
        }

        [Fact]
        public void SerializationWithoutNewLine()
        {
            var person = new PersonWithoutNewLine { Name = "PandaTea", Age = 100 };

            const string expected =
@"# The person's name
Name: PandaTea
# The person's age
Age: 100
";

            var result = new Serializer().Serialize(person);
            Output.WriteLine(result);

            result.Should().Be(expected);

            new Deserializer()
              .Deserialize<Person>(result)
              .ShouldBeEquivalentTo(person);
        }

        [Fact]
        public void SerializationWithNewLine()
        {
            var person = new Person { Name = "PandaTea", Age = 100 };

            const string expected =
@"# The person's name
Name: PandaTea

# The person's age
Age: 100
";

            var result = new Serializer().Serialize(person);
            Output.WriteLine(result);

            result.Should().Be(expected);

            new Deserializer()
                .Deserialize<Person>(result)
                .ShouldBeEquivalentTo(person);
        }

        [Fact]
        public void SerializationWithNewLine_IndentedInSequence()
        {
            var persons = new Person[] { new Person { Name = "PandaTea", Age = 100 } };

            const string expected =
@"- # The person's name
  Name: PandaTea

  # The person's age
  Age: 100
";

            var result = new Serializer().Serialize(persons);
            Output.WriteLine(result);

            result.Should().Be(expected);

            new Deserializer()
              .Deserialize<Person[]>(result)
              .ShouldBeEquivalentTo(persons);
        }

        private class Person
        {
            [YamlMember(Description = "The person's name")]
            public string Name { get; set; }

            [YamlMember(Description = "The person's age", NewLine = true)]
            public int Age { get; set; }
        }

        private class PersonWithoutNewLine
        {
            [YamlMember(Description = "The person's name")]
            public string Name { get; set; }

            [YamlMember(Description = "The person's age")]
            public int Age { get; set; }
        }
    }
}
