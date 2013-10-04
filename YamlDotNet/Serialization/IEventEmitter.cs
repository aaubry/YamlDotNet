namespace YamlDotNet.Serialization
{
	/// <summary>
	/// Interface used to write YAML events.
	/// </summary>
	public interface IEventEmitter
	{
		void Emit(AliasEventInfo eventInfo);

		void Emit(ScalarEventInfo eventInfo);

		void Emit(MappingStartEventInfo eventInfo);

		void Emit(MappingEndEventInfo eventInfo);

		void Emit(SequenceStartEventInfo eventInfo);

		void Emit(SequenceEndEventInfo eventInfo);
	}
}