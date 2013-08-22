using Xunit;
using Xunit.Extensions;
using YamlDotNet.RepresentationModel;
using YamlDotNet.RepresentationModel.Serialization;
using YamlDotNet.RepresentationModel.Serialization.NamingConventions;

namespace YamlDotNet.UnitTests.RepresentationModel
{
	public class NamingConventionTests
	{
		[Theory]
		[InlineData("test", "test")]
		[InlineData("thisIsATest", "this-is-a-test")]
		[InlineData("thisIsATest", "this_is_a_test")]
		[InlineData("thisIsATest", "ThisIsATest")]
		public void TestCamelCase(string expected, string input)
		{
			var sut = new CamelCaseNamingConvention();
			Assert.Equal(expected, sut.Apply(input));
		}

		[Theory]
		[InlineData("Test", "test")]
		[InlineData("ThisIsATest", "this-is-a-test")]
		[InlineData("ThisIsATest", "this_is_a_test")]
		[InlineData("ThisIsATest", "thisIsATest")]
		public void TestPascalCase(string expected, string input)
		{
			var sut = new PascalCaseNamingConvention();
			Assert.Equal(expected, sut.Apply(input));
		}

		[Theory]
		[InlineData("test", "test")]
		[InlineData("this-is-a-test", "thisIsATest")]
		[InlineData("this-is-a-test", "this-is-a-test")]
		public void TestHyphenated(string expected, string input)
		{
			var sut = new HyphenatedNamingConvention();
			Assert.Equal(expected, sut.Apply(input));
		}

		[Theory]
		[InlineData("test", "test")]
		[InlineData("this_is_a_test", "thisIsATest")]
		[InlineData("this_is_a_test", "this-is-a-test")]
		public void TestUnderscored(string expected, string input)
		{
			var sut = new UnderscoredNamingConvention();
			Assert.Equal(expected, sut.Apply(input));
		}
	}
}
