
namespace YamlDotNet.RepresentationModel.Serialization.NamingConventions
{
	/// <summary>
	/// Convert the string with underscores (this_is_a_test) or hyphens (this-is-a-test) to 
	/// camel case (thisIsATest). Camel case is the same as Pascal case, except the first letter
	/// is lowercase.
	/// </summary>
	public sealed class CamelCaseNamingConvention : INamingConvention
	{
		public string Apply(string value)
		{
			return value.ToCamelCase();
		}
	}
}
