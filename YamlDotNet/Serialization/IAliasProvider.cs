namespace YamlDotNet.RepresentationModel.Serialization
{
	public interface IAliasProvider
	{
		string GetAlias(object target);
	}
}