
namespace YamlDotNet.RepresentationModel.Serialization.NamingConventions
{
	/// <summary>
	/// Convert the string with underscores (this_is_a_test) or hyphens (this-is-a-test) to 
	/// pascal case (ThisIsATest). Pascal case is the same as camel case, except the first letter
	/// is uppercase.
	/// </summary>
	public sealed class PascalCaseNamingConvention : INamingConvention
	{
		public string Apply(string value)
		{
			return value.ToPascalCase();
		}
	}
}
