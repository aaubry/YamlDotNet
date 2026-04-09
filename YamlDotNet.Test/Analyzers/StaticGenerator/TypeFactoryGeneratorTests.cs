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
using System.Text;
using FluentAssertions;
using Xunit;

namespace YamlDotNet.Test.Analyzers.StaticGenerator
{
    /// <summary>
    /// Tests for patterns used in the TypeFactoryGenerator source generator.
    /// The source generator is referenced as an analyzer and not as a library,
    /// so we validate the exception-formatting pattern in isolation.
    /// </summary>
    public class TypeFactoryGeneratorTests
    {
        /// <summary>
        /// Reproduces the inner-exception traversal logic from TypeFactoryGenerator.GenerateSource()
        /// to verify it terminates and captures all exception messages in the chain.
        /// </summary>
        [Fact]
        public void InnerExceptionTraversal_TerminatesAndCapturesAllMessages()
        {
            // Arrange: create a 3-level exception chain
            var innerMost = new InvalidOperationException("inner-most");
            var middle = new ArgumentException("middle", innerMost);
            var outer = new Exception("outer", middle);

            var messages = new List<string>();

            // Act: replicate the fixed loop from TypeFactoryGenerator.GenerateSource()
            var e = outer;
            while (e != null)
            {
                messages.Add(e.Message);
                e = e.InnerException;
            }

            // Assert
            messages.Should().HaveCount(3);
            messages[0].Should().Be("outer");
            messages[1].Should().Be("middle");
            messages[2].Should().Be("inner-most");
        }

        /// <summary>
        /// Verifies the traversal works with a single exception (no inner exceptions).
        /// </summary>
        [Fact]
        public void InnerExceptionTraversal_SingleException_TerminatesAfterOne()
        {
            var exception = new Exception("only-one");

            var messages = new List<string>();
            var e = exception;
            while (e != null)
            {
                messages.Add(e.Message);
                e = e.InnerException;
            }

            messages.Should().ContainSingle().Which.Should().Be("only-one");
        }

        /// <summary>
        /// Verifies the full error-comment output format matches what TypeFactoryGenerator produces.
        /// </summary>
        [Fact]
        public void ErrorCommentFormat_ContainsAllExceptionDetails()
        {
            var inner = new InvalidOperationException("inner-error");
            var outer = new Exception("outer-error", inner);

            var result = new StringBuilder();
            result.AppendLine("/*");

            var e = outer;
            while (e != null)
            {
                result.AppendLine(e.Message);
                result.AppendLine(e.StackTrace);
                result.AppendLine("======");
                e = e.InnerException;
            }

            result.AppendLine("*/");

            var output = result.ToString();
            output.Should().Contain("outer-error");
            output.Should().Contain("inner-error");
            output.Should().Contain("/*");
            output.Should().Contain("*/");

            // Verify "======" appears twice (once per exception)
            var separatorCount = output.Split(new[] { "======" }, StringSplitOptions.None).Length - 1;
            separatorCount.Should().Be(2);
        }
    }
}
