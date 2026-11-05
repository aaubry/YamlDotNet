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
using System.Linq;
using FluentAssertions;
using Xunit;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.TypeResolvers;

namespace YamlDotNet.Test.Serialization
{
    public class RectangularArrayTests
    {
        private static readonly ISerializer Serializer = new SerializerBuilder().WithNewLine("\n").Build();
        private static readonly IDeserializer Deserializer = new DeserializerBuilder().Build();

        [Fact]
        public void RankTwoUsesNestedSequences()
        {
            var input = new[,] { { 1, 2 }, { 3, 4 } };
            var yaml = Serializer.Serialize(input);

            yaml.Should().Be("- - 1\n  - 2\n- - 3\n  - 4\n");
            var actual = Deserializer.Deserialize<int[,]>(yaml);
            actual.GetLength(0).Should().Be(2);
            actual.GetLength(1).Should().Be(2);
            actual.Cast<int>().Should().Equal(1, 2, 3, 4);
        }

        [Fact]
        public void RankThreeKeepsEveryAxisInRowMajorOrder()
        {
            var input = new[, ,] { { { 1, 2, 3 }, { 4, 5, 6 } }, { { 7, 8, 9 }, { 10, 11, 12 } } };
            var actual = Deserializer.Deserialize<int[,,]>(Serializer.Serialize(input));

            actual.GetLength(0).Should().Be(2);
            actual.GetLength(1).Should().Be(2);
            actual.GetLength(2).Should().Be(3);
            actual.Cast<int>().Should().Equal(Enumerable.Range(1, 12));
        }

        [Fact]
        public void DeclaredElementTypesWorkWithStaticTypeResolution()
        {
            var serializer = new SerializerBuilder().WithTypeResolver(new StaticTypeResolver()).Build();
            var input = new[,] { { 1, 2, 3 } };
            var actual = Deserializer.Deserialize<int[,]>(serializer.Serialize(input));

            actual.GetLength(0).Should().Be(1);
            actual.GetLength(1).Should().Be(3);
            actual.Cast<int>().Should().Equal(1, 2, 3);
        }

        [Fact]
        public void ScalarConversionsUseTheDeclaredElementType()
        {
            var actual = Deserializer.Deserialize<int[,]>("- ['1', '2']\n- ['3', '4']\n");

            actual.Cast<int>().Should().Equal(1, 2, 3, 4);
        }

        [Fact]
        public void NullAndReferenceElementsRoundtrip()
        {
            var input = new string[,] { { null, "value" } };
            var actual = Deserializer.Deserialize<string[,]>(Serializer.Serialize(input));

            actual[0, 0].Should().BeNull();
            actual[0, 1].Should().Be("value");
        }

        [Fact]
        public void AnAliasedRowContributesItsValuesToEachPosition()
        {
            var actual = Deserializer.Deserialize<int[,]>("- &row [1, 2]\n- *row\n");

            actual.GetLength(0).Should().Be(2);
            actual.GetLength(1).Should().Be(2);
            actual.Cast<int>().Should().Equal(1, 2, 1, 2);
        }

        [Fact]
        public void ReferenceElementAliasesKeepTheirIdentity()
        {
            var item = new ArrayItem { Name = "same" };
            var actual = Deserializer.Deserialize<ArrayItem[,]>(Serializer.Serialize(new[,] { { item, item } }));

            actual[0, 0].Name.Should().Be("same");
            actual[0, 1].Should().BeSameAs(actual[0, 0]);
        }

        [Fact]
        public void RootSelfReferenceResolvesIntoTheDestinationArray()
        {
            var input = new object[1, 1];
            input[0, 0] = input;
            var actual = Deserializer.Deserialize<object[,]>(Serializer.Serialize(input));

            actual[0, 0].Should().BeSameAs(actual);
        }

        [Fact]
        public void RootSelfReferenceUpdatesEveryAliasedRow()
        {
            var actual = Deserializer.Deserialize<object[,]>("&root\n- &row [*root]\n- *row\n");

            actual.GetLength(0).Should().Be(2);
            actual.GetLength(1).Should().Be(1);
            actual[0, 0].Should().BeSameAs(actual);
            actual[1, 0].Should().BeSameAs(actual);
        }

        [Fact]
        public void RankThreeSelfReferenceUpdatesAliasedInnermostRows()
        {
            var actual = Deserializer.Deserialize<object[,,]>("&root\n- [&row [*root]]\n- [*row]\n");

            actual.GetLength(0).Should().Be(2);
            actual.GetLength(1).Should().Be(1);
            actual.GetLength(2).Should().Be(1);
            actual[0, 0, 0].Should().BeSameAs(actual);
            actual[1, 0, 0].Should().BeSameAs(actual);
        }

        [Fact]
        public void RepeatedRectangularArraysKeepTheirIdentity()
        {
            var matrix = new[,] { { 1, 2 } };
            var actual = Deserializer.Deserialize<ArrayHolder>(Serializer.Serialize(new ArrayHolder { First = matrix, Second = matrix }));

            actual.Second.Should().BeSameAs(actual.First);
            actual.First.Cast<int>().Should().Equal(1, 2);
        }

        [Fact]
        public void RaggedInnermostDimensionsAreRejected()
        {
            Action deserialize = () => Deserializer.Deserialize<int[,,]>("- [[1, 2]]\n- [[3]]\n");

            deserialize.Should().Throw<YamlException>().WithMessage("*Dimension 3*inconsistent*");
        }

        [Theory]
        [InlineData("- [1, 2]\n- [3]\n")]
        [InlineData("- null\n")]
        [InlineData("- 1\n- 2\n")]
        [InlineData("- [[1]]\n")]
        public void InvalidRectangularShapesAreRejected(string yaml)
        {
            Action deserialize = () => Deserializer.Deserialize<int[,]>(yaml);

            deserialize.Should().Throw<YamlException>();
        }

        [Theory]
        [InlineData(0, 3, 0, 0)]
        [InlineData(2, 0, 2, 0)]
        [InlineData(0, 0, 0, 0)]
        public void EmptyAxesPreserveTheShapeRepresentableBySequences(int first, int second, int expectedFirst, int expectedSecond)
        {
            var actual = Deserializer.Deserialize<int[,]>(Serializer.Serialize(new int[first, second]));

            actual.GetLength(0).Should().Be(expectedFirst);
            actual.GetLength(1).Should().Be(expectedSecond);
        }

        [Theory]
        [InlineData(2, 0, 4, 2, 0, 0)]
        [InlineData(2, 3, 0, 2, 3, 0)]
        [InlineData(0, 3, 4, 0, 0, 0)]
        public void AxesHiddenAfterAnEmptyAxisNormalizeToZero(int first, int second, int third,
            int expectedFirst, int expectedSecond, int expectedThird)
        {
            var actual = Deserializer.Deserialize<int[,,]>(Serializer.Serialize(new int[first, second, third]));

            actual.GetLength(0).Should().Be(expectedFirst);
            actual.GetLength(1).Should().Be(expectedSecond);
            actual.GetLength(2).Should().Be(expectedThird);
        }

        [Fact]
        public void NonZeroLowerBoundsAreRejectedInsteadOfBeingLost()
        {
            var input = Array.CreateInstance(typeof(int), new[] { 2, 2 }, new[] { 1, 1 });
            Action serialize = () => Serializer.Serialize(input);

            serialize.Should().Throw<NotSupportedException>().WithMessage("*zero-based*");
        }

        [Fact]
        public void RankOneAndJaggedArraysKeepTheirExistingBehavior()
        {
            Deserializer.Deserialize<int[]>(Serializer.Serialize(new[] { 1, 2 })).Should().Equal(1, 2);
            var input = new[] { new[] { 1, 2 }, new[] { 3 } };
            var actual = Deserializer.Deserialize<int[][]>(Serializer.Serialize(input));

            actual.Should().HaveCount(2);
            actual[0].Should().Equal(1, 2);
            actual[1].Should().Equal(3);
        }

        public class ArrayItem
        {
            public string Name { get; set; }
        }

        public class ArrayHolder
        {
            public int[,] First { get; set; }
            public int[,] Second { get; set; }
        }
    }
}
