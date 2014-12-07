//  This file is part of YamlDotNet - A .NET library for YAML.
//  Copyright (c) Antoine Aubry and contributors
    
//  Permission is hereby granted, free of charge, to any person obtaining a copy of
//  this software and associated documentation files (the "Software"), to deal in
//  the Software without restriction, including without limitation the rights to
//  use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
//  of the Software, and to permit persons to whom the Software is furnished to do
//  so, subject to the following conditions:
    
//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.
    
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.

using FluentAssertions;
using Xunit.Extensions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace YamlDotNet.Test.Serialization
{
	public class NamingConventionTests
	{
		[Theory]
		[InlineData("test", "test")]
		[InlineData("thisIsATest", "this-is-a-test")]
		[InlineData("thisIsATest", "this_is_a_test")]
		[InlineData("thisIsATest", "ThisIsATest")]
		public void AppliesCamelCaseConvention(string expectedName, string input)
		{
			ShouldApplyConventionGiven(input, expectedName, new CamelCaseNamingConvention());
		}

		[Theory]
		[InlineData("Test", "test")]
		[InlineData("ThisIsATest", "this-is-a-test")]
		[InlineData("ThisIsATest", "this_is_a_test")]
		[InlineData("ThisIsATest", "thisIsATest")]
		public void AppliesPascalCaseConvention(string expectedName, string input)
		{
			ShouldApplyConventionGiven(input, expectedName, new PascalCaseNamingConvention());
		}

		[Theory]
		[InlineData("test", "test")]
		[InlineData("this-is-a-test", "thisIsATest")]
		[InlineData("this-is-a-test", "this-is-a-test")]
		public void AppliesHyphenatedConvention(string expectedName, string input)
		{
			ShouldApplyConventionGiven(input, expectedName, new HyphenatedNamingConvention());
		}

		[Theory]
		[InlineData("test", "test")]
		[InlineData("this_is_a_test", "thisIsATest")]
		[InlineData("this_is_a_test", "this-is-a-test")]
		public void AppliesUnderscoredConvention(string expectedName, string input)
		{
			ShouldApplyConventionGiven(input, expectedName, new UnderscoredNamingConvention());
		}

		private void ShouldApplyConventionGiven(string input, string expectedName, INamingConvention convention)
		{
			convention.Apply(input).Should().Be(expectedName);
		}
	}
}
