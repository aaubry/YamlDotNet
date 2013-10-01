using YamlDotNet.Events;

namespace YamlDotNet
{
	/// <summary>
	/// Represents a YAML stream emitter.
	/// </summary>
	public interface IEmitter
	{
		/// <summary>
		/// Emits an event.
		/// </summary>
		void Emit(IParsingEvent @event);
	}
}
