using Xunit;
using Xunit.Extensions;
using YamlDotNet.RepresentationModel;

namespace YamlDotNet.UnitTests.RepresentationModel
{
	public class StringExtensionTests
	{
		[Theory]
		[InlineData("test", "test")]
		[InlineData("thisIsATest", "this-is-a-test")]
		[InlineData("thisIsATest", "this_is_a_test")]
		public void TestToCamelCase(string expected, string input)
		{
			Assert.Equal(expected, input.ToCamelCase());
		}

		[Theory]
		[InlineData("Test", "test")]
		[InlineData("ThisIsATest", "this-is-a-test")]
		[InlineData("ThisIsATest", "this_is_a_test")]
		public void TestToPascalCase(string expected, string input)
		{
			Assert.Equal(expected, input.ToPascalCase());
		}

		[Theory]
		[InlineData("test", "test")]
		[InlineData("this-is-a-test", "thisIsATest")]
		[InlineData("this-is-a-test", "this-is-a-test")]
		public void TestFromCamelCase(string expected, string input)
		{
			Assert.Equal(expected, input.FromCamelCase("-"));
		}
	}
}
