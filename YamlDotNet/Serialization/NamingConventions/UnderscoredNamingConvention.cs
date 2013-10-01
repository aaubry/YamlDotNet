
namespace YamlDotNet.RepresentationModel.Serialization.NamingConventions
{
	/// <summary>
	/// Convert the string from camelcase (thisIsATest) to a underscored (this_is_a_test) string
	/// </summary>
	public sealed class UnderscoredNamingConvention : INamingConvention
	{
		public string Apply(string value)
		{
			return value.FromCamelCase("_");
		}
	}
}
