namespace YamlDotNet.Serialization
{
	public interface IAliasProvider
	{
		string GetAlias(object target);
	}
}