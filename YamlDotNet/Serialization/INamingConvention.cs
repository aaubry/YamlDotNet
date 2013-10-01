
namespace YamlDotNet.RepresentationModel.Serialization
{
	/// <summary>
	/// Translates property names according to a specific convention.
	/// </summary>
	public interface INamingConvention
	{
		string Apply(string value);
	}
}
