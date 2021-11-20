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
using System.Collections.Generic;
using FluentAssertions;
using Xunit;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

#nullable enable
namespace YamlDotNet.Test.Serialization
{
    public class DeserializerReadOnlyTest
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
- year: 2021
  name: Honda
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
            person.Cars[0].Spec.Should().NotBeNull();
            person.Cars[0].Spec!.EngineType.Should().Be("V6");
            person.Cars[0].Spec!.DriveType.Should().Be("AWD");
            person.Cars[1].Name.Should().Be("Honda");
            person.Cars[1].Spec.Should().NotBeNull();
            person.Cars[1].Spec!.EngineType.Should().Be("V4");
            person.Cars[1].Spec!.DriveType.Should().Be("FWD");
        }

        [Fact]
        public void Deserialize_YamlWithMissingParameter_ThrowsError()
        {
            var yaml = "{ x: 1 }";
            var sut = new DeserializerBuilder().Build();

            Action action = () => sut.Deserialize<RequiresTwoParameters>(yaml);
            action.ShouldThrow<YamlException>();
        }

        [Fact]
        public void Deserialize_YamlWithOutOfOrderParameters_ReturnsModel()
        {
            var yaml = "{ y: 2, x: 1 }";
            var sut = new DeserializerBuilder().Build();

            var actual = sut.Deserialize<OrderIndependentParams>(yaml);
            actual.x.Should().Be(1);
            actual.y.Should().Be(2);
        }

        [Fact]
        public void Deserialize_YamlWithExtraParametersAndIgnoreUnmatched_ReturnsModel()
        {
            var yaml = "{ y: 2, x: 1, z: 3 }";
            var sut = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();

            var actual = sut.Deserialize<OrderIndependentParams>(yaml);
            actual.x.Should().Be(1);
            actual.y.Should().Be(2);
        }

        [Fact]
        public void Deserialize_YamlWithExtraParametersAndNoIgnoreUnmatched_ThrowsError()
        {
            var yaml = "{ y: 2, x: 1, z: 3 }";
            var sut = new DeserializerBuilder().Build();

            Action action = () => sut.Deserialize<OrderIndependentParams>(yaml);
            action.ShouldThrow<YamlException>();
        }

        [Fact]
        public void Deserialize_WithConstructorThrowingExceptionWrapsAndDescribes()
        {
            var yaml = "{ anInt: 2, aString: TheString }";
            var sut = new DeserializerBuilder().Build();

            Action action = () => sut.Deserialize<ConstructorThrowsException>(yaml);
            action.ShouldThrow<YamlException>();
        }

        [Fact]
        public void Deserialize_YamlWithOptionalParameterMissing_ReturnsModel()
        {
            var yaml = @"
requiredInt: 42
";
            var sut = new DeserializerBuilder().Build();

            var actual = sut.Deserialize<OptionalConstructorParams>(yaml);
            actual.requiredInt.Should().Be(42);
            actual.optionalString.Should().Be("default value");
            actual.optionalFloat.Should().NotHaveValue();
        }

        [Fact]
        public void Deserialize_YamlWithOptionalParameterPresent_ReturnsModel()
        {
            var yaml = @"
requiredInt: 42
optionalString: present
optionalFloat: 3.14
";
            var sut = new DeserializerBuilder().Build();

            var actual = sut.Deserialize<OptionalConstructorParams>(yaml);
            actual.requiredInt.Should().Be(42);
            actual.optionalString.Should().Be("present");
            actual.optionalFloat.Should().Be(3.14f);
        }

        private class Person
        {
            public string Name { get; }

            public DateTime MomentOfBirth { get; }

            public IList<ICar> Cars { get; }

            [YamlConstructor]
            public Person(string name, DateTime momentOfBirth, IList<ICar> cars)
            {
                Name = name;
                MomentOfBirth = momentOfBirth;
                Cars = cars;
            }
        }

        private class Car : ICar
        {
            public string Name { get; }

            public int Year { get; }

            public IModelSpec? Spec { get; }

            [YamlConstructor]
            public Car(string name, int year, IModelSpec? spec = null)
            {
                Name = name;
                Year = year;
                Spec = spec;
            }
        }

        private interface ICar
        {
            string Name { get; }

            int Year { get; }
            IModelSpec? Spec { get; }
        }

        private class ModelSpec : IModelSpec
        {
            public string EngineType { get; }

            public string DriveType { get; }

            [YamlConstructor]
            public ModelSpec(string engineType, string driveType)
            {
                EngineType = engineType;
                DriveType = driveType;
            }
        }

        private interface IModelSpec
        {
            string EngineType { get; }

            string DriveType { get; }
        }

        private class RequiresTwoParameters
        {
            public readonly int x;

            public readonly int y;

            [YamlConstructor]
            public RequiresTwoParameters(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        private class OrderIndependentParams
        {
            public readonly int x;

            public readonly int y;

            [YamlConstructor]
            public OrderIndependentParams(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        private class OptionalConstructorParams
        {
            public readonly int requiredInt;

            public readonly string? optionalString;

            public readonly float? optionalFloat;

            [YamlConstructor]
            public OptionalConstructorParams(int requiredInt, string? optionalString = "default value", float? optionalFloat = float.NaN)
            {
                this.requiredInt = requiredInt;
                this.optionalString = optionalString;
                if (optionalFloat.HasValue && !float.IsNaN(optionalFloat.Value))
                {
                    this.optionalFloat = optionalFloat;
                }
            }
        }

        private class ConstructorThrowsException
        {
            public readonly int anInt;

            public readonly string aString;

            [YamlConstructor]
            public ConstructorThrowsException(int anInt, string aString)
            {
                this.anInt = anInt;
                this.aString = aString;
                throw new Exception("Throw to test exception wrapping");
            }
        }

    }
}
