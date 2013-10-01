
namespace YamlDotNet.RepresentationModel.Serialization.NamingConventions
{
	/// <summary>
	/// Convert the string from camelcase (thisIsATest) to a hyphenated (this-is-a-test) string
	/// </summary>
	public sealed class HyphenatedNamingConvention : INamingConvention
	{
		public string Apply(string value)
		{
			return value.FromCamelCase("-");
		}
	}
}
