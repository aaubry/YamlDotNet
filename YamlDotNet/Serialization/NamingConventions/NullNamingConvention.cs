
namespace YamlDotNet.RepresentationModel.Serialization.NamingConventions
{
	/// <summary>
	/// Performs no naming conversion.
	/// </summary>
	public sealed class NullNamingConvention : INamingConvention
	{
		public string Apply(string value)
		{
			return value;
		}
	}
}
