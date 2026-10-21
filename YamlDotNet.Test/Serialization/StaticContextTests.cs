// This file is part of YamlDotNet - A .NET library for YAML.
// Copyright (c) Antoine Aubry and contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
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
using Xunit;
using YamlDotNet.Serialization;

namespace YamlDotNet.Test.Serialization
{
    public class StaticContextTests
    {
        [Fact]
        public void MissingGeneratedImplementationExplainsHowToConfigureTheSourceGenerator()
        {
            var context = new CustomStaticContextWithoutGeneratedCode();

            AssertMissingGenerationGuidance(() => { context.IsKnownType(typeof(string)); });
            AssertMissingGenerationGuidance(() => { context.GetTypeResolver(); });
            AssertMissingGenerationGuidance(() => { context.GetFactory(); });
            AssertMissingGenerationGuidance(() => { context.GetTypeInspector(); });
            AssertMissingGenerationGuidance(() => { new StaticDeserializerBuilder(context); });
        }

        private static void AssertMissingGenerationGuidance(Action action)
        {
            var exception = Assert.Throws<NotImplementedException>(action);

            Assert.Contains("Static source generation has not been applied", exception.Message);
            Assert.Contains("top-level partial class", exception.Message);
            Assert.Contains("[YamlStaticContext]", exception.Message);
            Assert.Contains("[YamlSerializable(typeof(...))]", exception.Message);
            Assert.Contains("YamlDotNet.Analyzers.StaticGenerator", exception.Message);
        }

        private sealed class CustomStaticContextWithoutGeneratedCode : StaticContext
        {
        }
    }
}
