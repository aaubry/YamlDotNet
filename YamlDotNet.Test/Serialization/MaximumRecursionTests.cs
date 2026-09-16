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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace YamlDotNet.Test.Serialization;

public class MaximumRecursionTests
{
    [Fact]
    public void Given_DeeplyNestedObject_Serializer_Should_Throw_MaximumRecursionLevelReachedException_When_WithMaximumRecursion_Is_Specified()
    {
        var deeplyNested = Enumerable.Range(1, 10)
            .Aggregate(Array.Empty<object>(), (root, _) => [root]);

        var sut = new SerializerBuilder()
            .WithMaximumRecursion(5)
            .Build();

        Assert.Throws<MaximumRecursionLevelReachedException>(() => sut.Serialize(deeplyNested));
    }

    [Fact]
    public void Given_DeeplyNestedSequence_Deserializer_Should_Throw_MaximumRecursionLevelReachedException_When_WithMaximumRecursion_Is_Specified()
    {
        var deeplyNested = "[[[[[[[[[[]]]]]]]]]]";

        var sut = new DeserializerBuilder()
            .WithMaximumRecursion(5)
            .Build();

        Assert.Throws<MaximumRecursionLevelReachedException>(() => sut.Deserialize(deeplyNested));
    }

    [Fact]
    public void Given_DeeplyNestedMapping_Deserializer_Should_Throw_MaximumRecursionLevelReachedException_When_WithMaximumRecursion_Is_Specified()
    {
        var deeplyNested = "{ a: { a: { a: { a: { a: { a: { a: 0 } } } } } } }";

        var sut = new DeserializerBuilder()
            .WithMaximumRecursion(5)
            .Build();

        Assert.Throws<MaximumRecursionLevelReachedException>(() => sut.Deserialize(deeplyNested));
    }

    [Fact]
    public void Given_DeeplyNestedMappingAndSequenceMix_Deserializer_Should_Throw_MaximumRecursionLevelReachedException_When_WithMaximumRecursion_Is_Specified()
    {
        var deeplyNested = "[ { a: [ { a: { a: 0 } } ] } ]";

        var sut = new DeserializerBuilder()
            .WithMaximumRecursion(5)
            .Build();

        Assert.Throws<MaximumRecursionLevelReachedException>(() => sut.Deserialize(deeplyNested));
    }
}
