namespace YamlDotNet.Serialization
{
	public interface ISerializationBehaviorFactory
	{
		ISerializationBehavior Create();
	}
}