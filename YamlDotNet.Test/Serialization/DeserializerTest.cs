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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Xunit;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.Callbacks;
using YamlDotNet.Serialization.NamingConventions;

namespace YamlDotNet.Test.Serialization
{
    public class DeserializerTest
    {
        [Fact]
        public void Deserialize_YamlWithInterfaceTypeAndMapping_ReturnsModel()
        {
            var yaml = @"
name: Jack
momentOfBirth: 1983-04-21T20:21:03.0041599Z
cars:
- name: Mercedes
  year: 2018
- name: Honda
  year: 2021
";

            var sut = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithTypeMapping<ICar, Car>()
                .Build();

            var person = sut.Deserialize<Person>(yaml);
            person.Name.Should().Be("Jack");
            person.MomentOfBirth.Kind.Should().Be(DateTimeKind.Utc);
            person.MomentOfBirth.ToUniversalTime().Year.Should().Be(1983);
            person.MomentOfBirth.ToUniversalTime().Month.Should().Be(4);
            person.MomentOfBirth.ToUniversalTime().Day.Should().Be(21);
            person.MomentOfBirth.ToUniversalTime().Hour.Should().Be(20);
            person.MomentOfBirth.ToUniversalTime().Minute.Should().Be(21);
            person.MomentOfBirth.ToUniversalTime().Second.Should().Be(3);
            person.Cars.Should().HaveCount(2);
            person.Cars[0].Name.Should().Be("Mercedes");
            person.Cars[0].Spec.Should().BeNull();
            person.Cars[1].Name.Should().Be("Honda");
            person.Cars[1].Spec.Should().BeNull();
        }

        [Fact]
        public void Deserialize_YamlWithTwoInterfaceTypesAndMappings_ReturnsModel()
        {
            var yaml = @"
name: Jack
momentOfBirth: 1983-04-21T20:21:03.0041599Z
cars:
- name: Mercedes
  year: 2018
  spec:
    engineType: V6
    driveType: AWD
- name: Honda
  year: 2021
  spec:
    engineType: V4
    driveType: FWD
";

            var sut = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithTypeMapping<ICar, Car>()
                .WithTypeMapping<IModelSpec, ModelSpec>()
                .Build();

            var person = sut.Deserialize<Person>(yaml);
            person.Name.Should().Be("Jack");
            person.MomentOfBirth.Kind.Should().Be(DateTimeKind.Utc);
            person.MomentOfBirth.ToUniversalTime().Year.Should().Be(1983);
            person.MomentOfBirth.ToUniversalTime().Month.Should().Be(4);
            person.MomentOfBirth.ToUniversalTime().Day.Should().Be(21);
            person.MomentOfBirth.ToUniversalTime().Hour.Should().Be(20);
            person.MomentOfBirth.ToUniversalTime().Minute.Should().Be(21);
            person.MomentOfBirth.ToUniversalTime().Second.Should().Be(3);
            person.Cars.Should().HaveCount(2);
            person.Cars[0].Name.Should().Be("Mercedes");
            person.Cars[0].Spec.EngineType.Should().Be("V6");
            person.Cars[0].Spec.DriveType.Should().Be("AWD");
            person.Cars[1].Name.Should().Be("Honda");
            person.Cars[1].Spec.EngineType.Should().Be("V4");
            person.Cars[1].Spec.DriveType.Should().Be("FWD");
        }

        [Fact]
        public void SetterOnlySetsWithoutException()
        {
            var yaml = @"
Value: bar
";
            var deserializer = new DeserializerBuilder().Build();
            var result = deserializer.Deserialize<SetterOnly>(yaml);
            result.Actual.Should().Be("bar");
        }

        [Fact]
        public void KeysOnDynamicClassDontGetQuoted()
        {
            var serializer = new SerializerBuilder().WithQuotingNecessaryStrings().Build();
            var deserializer = new DeserializerBuilder().WithAttemptingUnquotedStringTypeDeserialization().Build();
            var yaml = @"
True: null
False: hello
Null: true
X:
";
            var obj = deserializer.Deserialize(yaml, typeof(object));
            var result = serializer.Serialize(obj);
            var dictionary = (Dictionary<object, object>)obj;
            var keys = dictionary.Keys.ToArray();
            Assert.Equal(keys, new[] { "True", "False", "Null", "X" });
            Assert.Equal(dictionary.Values, new object[] { null, "hello", true, null });
        }

        [Fact]
        public void EmptyQuotedStringsArentNull()
        {
            var deserializer = new DeserializerBuilder().WithAttemptingUnquotedStringTypeDeserialization().Build();
            var yaml = "Value: \"\"";
            var result = deserializer.Deserialize<Test>(yaml);
            Assert.Equal(string.Empty, result.Value);
        }

        [Fact]
        public void KeyAnchorIsHandledWithTypeDeserialization()
        {
            var yaml = @"a: &some_scalar this is also a key
b: &number 1
*some_scalar: ""will this key be handled correctly?""
*number: 1";
            var deserializer = new DeserializerBuilder().WithAttemptingUnquotedStringTypeDeserialization().Build();
            var result = deserializer.Deserialize(yaml, typeof(object));
            Assert.IsType<Dictionary<object, object>>(result);
            var dictionary = (Dictionary<object, object>)result;
            Assert.Equal(new object[] { "a", "b", "this is also a key", (byte)1 }, dictionary.Keys);
            Assert.Equal(new object[] { "this is also a key", (byte)1, "will this key be handled correctly?", (byte)1 }, dictionary.Values);
        }

        [Fact]
        public void NonScalarKeyIsHandledWithTypeDeserialization()
        {
            var yaml = @"scalar: foo
{ a: mapping }: bar
[ a, sequence, 1 ]: baz";
            var deserializer = new DeserializerBuilder().WithAttemptingUnquotedStringTypeDeserialization().Build();
            var result = deserializer.Deserialize(yaml, typeof(object));
            Assert.IsType<Dictionary<object, object>>(result);

            var dictionary = (Dictionary<object, object>)result;
            var item = dictionary.ElementAt(0);
            Assert.Equal("scalar", item.Key);
            Assert.Equal("foo", item.Value);

            item = dictionary.ElementAt(1);
            Assert.IsType<Dictionary<object, object>>(item.Key);
            Assert.Equal("bar", item.Value);
            dictionary = (Dictionary<object, object>)item.Key;
            item = dictionary.ElementAt(0);
            Assert.Equal("a", item.Key);
            Assert.Equal("mapping", item.Value);

            dictionary = (Dictionary<object, object>)result;
            item = dictionary.ElementAt(2);
            Assert.IsType<List<object>>(item.Key);
            Assert.Equal(new List<object> { "a", "sequence", (byte)1 }, (List<object>)item.Key);
            Assert.Equal("baz", item.Value);
        }

        [Fact]
        public void NewLinesInKeys()
        {
            var yaml = @"? >-
  key

  a

  b
: >-
  value

  a

  b
";
            var deserializer = new DeserializerBuilder().Build();
            var o = deserializer.Deserialize(yaml, typeof(object));
            Assert.IsType<Dictionary<object, object>>(o);
            var dictionary = (Dictionary<object, object>)o;
            Assert.Equal($"key\na\nb", dictionary.First().Key);
            Assert.Equal($"value\na\nb", dictionary.First().Value);
        }

        [Theory]
        [InlineData(".nan", System.Single.NaN)]
        [InlineData(".NaN", System.Single.NaN)]
        [InlineData(".NAN", System.Single.NaN)]
        [InlineData("-.inf", System.Single.NegativeInfinity)]
        [InlineData("+.inf", System.Single.PositiveInfinity)]
        [InlineData(".inf", System.Single.PositiveInfinity)]
        [InlineData("start.nan", "start.nan")]
        [InlineData(".nano", ".nano")]
        [InlineData(".infinity", ".infinity")]
        [InlineData("www.infinitetechnology.com", "www.infinitetechnology.com")]
        [InlineData("https://api.inference.azure.com", "https://api.inference.azure.com")]
        public void UnquotedStringTypeDeserializationHandlesInfAndNaN(string yamlValue, object expected)
        {
            var deserializer = new DeserializerBuilder()
                .WithAttemptingUnquotedStringTypeDeserialization().Build();
            var yaml = $"Value: {yamlValue}";

            var resultDict = deserializer.Deserialize<IDictionary<string, object>>(yaml);
            Assert.True(resultDict.ContainsKey("Value"));
            Assert.Equal(expected, resultDict["Value"]);
        }

        public static IEnumerable<object[]> DeserializeScalarEdgeCases_TestCases
        {
            get
            {
                yield return new object[] { byte.MinValue, typeof(byte) };
                yield return new object[] { byte.MaxValue, typeof(byte) };
                yield return new object[] { short.MinValue, typeof(short) };
                yield return new object[] { short.MaxValue, typeof(short) };
                yield return new object[] { int.MinValue, typeof(int) };
                yield return new object[] { int.MaxValue, typeof(int) };
                yield return new object[] { long.MinValue, typeof(long) };
                yield return new object[] { long.MaxValue, typeof(long) };
                yield return new object[] { sbyte.MinValue, typeof(sbyte) };
                yield return new object[] { sbyte.MaxValue, typeof(sbyte) };
                yield return new object[] { ushort.MinValue, typeof(ushort) };
                yield return new object[] { ushort.MaxValue, typeof(ushort) };
                yield return new object[] { uint.MinValue, typeof(uint) };
                yield return new object[] { uint.MaxValue, typeof(uint) };
                yield return new object[] { ulong.MinValue, typeof(ulong) };
                yield return new object[] { ulong.MaxValue, typeof(ulong) };
                yield return new object[] { decimal.MinValue, typeof(decimal) };
                yield return new object[] { decimal.MaxValue, typeof(decimal) };
                yield return new object[] { char.MaxValue, typeof(char) };

#if NETCOREAPP3_1_OR_GREATER
                yield return new object[] { float.MinValue, typeof(float) };
                yield return new object[] { float.MaxValue, typeof(float) };
                yield return new object[] { double.MinValue, typeof(double) };
                yield return new object[] { double.MaxValue, typeof(double) };
#endif
            }
        }

        [Theory]
        [MemberData(nameof(DeserializeScalarEdgeCases_TestCases))]
        public void DeserializeScalarEdgeCases(IConvertible value, Type type)
        {
            var deserializer = new DeserializerBuilder().Build();
            var result = deserializer.Deserialize(value.ToString(YamlFormatter.Default.NumberFormat), type);

            result.Should().Be(value);
        }

        [Fact]
        public void DeserializeWithDuplicateKeyChecking_YamlWithDuplicateKeys_ThrowsYamlException()
        {
            var yaml = @"
name: Jack
momentOfBirth: 1983-04-21T20:21:03.0041599Z
name: Jake
";

            var sut = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithDuplicateKeyChecking()
                .Build();

            Action act = () => sut.Deserialize<Person>(yaml);
            act.ShouldThrow<YamlException>("Because there are duplicate name keys with concrete class");
            act = () => sut.Deserialize<IDictionary<object, object>>(yaml);
            act.ShouldThrow<YamlException>("Because there are duplicate name keys with dictionary");

            var stream = Yaml.ReaderFrom("backreference.yaml");
            var parser = new MergingParser(new Parser(stream));
            act = () => sut.Deserialize<Dictionary<string, Dictionary<string, string>>>(parser);
            act.ShouldThrow<YamlException>("Because there are duplicate name keys with merging parser");
        }

        [Fact]
        public void DeserializeWithoutDuplicateKeyChecking_YamlWithDuplicateKeys_DoesNotThrowYamlException()
        {
            var yaml = @"
name: Jack
momentOfBirth: 1983-04-21T20:21:03.0041599Z
name: Jake
";

            var sut = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            Action act = () => sut.Deserialize<Person>(yaml);
            act.ShouldNotThrow<YamlException>("Because duplicate key checking is not enabled");
            act = () => sut.Deserialize<IDictionary<object, object>>(yaml);
            act.ShouldNotThrow<YamlException>("Because duplicate key checking is not enabled");

            var stream = Yaml.ReaderFrom("backreference.yaml");
            var parser = new MergingParser(new Parser(stream));
            act = () => sut.Deserialize<Dictionary<string, Dictionary<string, string>>>(parser);
            act.ShouldNotThrow<YamlException>("Because duplicate key checking is not enabled");
        }

        [Fact]
        public void SerializeStateMethodsGetCalledOnce()
        {
            var yaml = "Test: Hi";
            var deserializer = new DeserializerBuilder().Build();
            var test = deserializer.Deserialize<TestState>(yaml);

            Assert.Equal(1, test.OnDeserializedCallCount);
            Assert.Equal(1, test.OnDeserializingCallCount);
        }
        
        [Fact]
        public void DeserializeConcurrently()
        {
            var exceptions = new ConcurrentStack<Exception>();
            var runCount = 10;

            for (var i = 0; i < runCount; i++)
            {
                // Failures don't occur consistently - running repeatedly increases the chances
                RunTest();
            }
            
            Assert.Empty(exceptions);

            void RunTest()
            {
                var threadCount = 100;
                var threads = new List<Thread>();
                var control = new SemaphoreSlim(0, threadCount);

                var yaml = "Test: Hi";
                var deserializer = new DeserializerBuilder().Build();

                for (var i = 0; i < threadCount; i++)
                {
                    threads.Add(new Thread(Deserialize));
                }

                threads.ForEach(t => t.Start());
                // Each thread will wait for the semaphore before proceeding.
                // Release them all simultaneously to maximise concurrency
                control.Release(threadCount);
                threads.ForEach(t => t.Join());

                Assert.Empty(exceptions);
                return;

                void Deserialize()
                {
                    control.Wait();

                    try
                    {
                        var result = deserializer.Deserialize<TestState>(yaml);
                        result.Test.Should().Be("Hi");
                    }
                    catch (Exception e)
                    {
                        exceptions.Push(e.InnerException ?? e);
                    }
                }
            }
        }

        public class TestState
        {
            public int OnDeserializedCallCount { get; set; }
            public int OnDeserializingCallCount { get; set; }

            public string Test { get; set; } = string.Empty;

            [OnDeserialized]
            public void Deserialized() => OnDeserializedCallCount++;

            [OnDeserializing]
            public void Deserializing() => OnDeserializingCallCount++;
        }

        public class Test
        {
            public string Value { get; set; }
        }

        public class SetterOnly
        {
            private string _value;
            public string Value { set => _value = value; }
            public string Actual { get => _value; }
        }

        public class Person
        {
            public string Name { get; private set; }

            public DateTime MomentOfBirth { get; private set; }

            public IList<ICar> Cars { get; private set; }
        }

        public class Car : ICar
        {
            public string Name { get; private set; }

            public int Year { get; private set; }

            public IModelSpec Spec { get; private set; }
        }

        public interface ICar
        {
            string Name { get; }

            int Year { get; }
            IModelSpec Spec { get; }
        }

        public class ModelSpec : IModelSpec
        {
            public string EngineType { get; private set; }

            public string DriveType { get; private set; }
        }

        public interface IModelSpec
        {
            string EngineType { get; }

            string DriveType { get; }
        }
    }
}
