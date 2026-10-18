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
using YamlDotNet.Core.ObjectPool;

namespace YamlDotNet.Test.Core
{
    public class StringBuilderPoolTests
    {
        [Fact]
        public void ShouldRentEmptyBuilders()
        {
            using (var wrapper = StringBuilderPool.Rent())
            {
                wrapper.Builder.Length.Should().Be(0);
                wrapper.Builder.Append("some text");
            }

            using var rented = StringBuilderPool.Rent();
            rented.Builder.Length.Should().Be(0);
        }

        [Fact]
        public void ShouldReuseReturnedBuilderOnTheSameThread()
        {
            var first = StringBuilderPool.Rent();
            var builder = first.Builder;
            builder.Append("some text");
            first.Dispose();

            using var second = StringBuilderPool.Rent();
            second.Builder.Should().BeSameAs(builder);
            second.Builder.Length.Should().Be(0);
        }

        [Fact]
        public void ShouldRentDistinctBuildersWhileTheyAreInUse()
        {
            using var first = StringBuilderPool.Rent();
            using var second = StringBuilderPool.Rent();
            using var third = StringBuilderPool.Rent();
            using var fourth = StringBuilderPool.Rent();
            using var fifth = StringBuilderPool.Rent();

            var builders = new[] { first.Builder, second.Builder, third.Builder, fourth.Builder, fifth.Builder };
            for (var i = 0; i < builders.Length; i++)
            {
                builders[i].Append(i);
            }

            builders.Should().OnlyHaveUniqueItems(b => b.ToString());
        }

        [Fact]
        public void ShouldNotRetainBuildersThatGrewTooLarge()
        {
            var first = StringBuilderPool.Rent();
            var builder = first.Builder;
            builder.Append('x', 4096);
            first.Dispose();

            using var second = StringBuilderPool.Rent();
            second.Builder.Should().NotBeSameAs(builder);
        }
    }
}
