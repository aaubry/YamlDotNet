namespace YamlDotNet.RepresentationModel.Serialization
{
	public interface ISerializationBehaviorFactory
	{
		ISerializationBehavior Create();
	}
}