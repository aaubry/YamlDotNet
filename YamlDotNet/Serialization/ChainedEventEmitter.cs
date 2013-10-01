using System;

namespace YamlDotNet.RepresentationModel.Serialization
{
	/// <summary>
	/// Provided the base implementation for an IEventEmitter that is a
	/// decorator for another IEventEmitter.
	/// </summary>
	public abstract class ChainedEventEmitter : IEventEmitter
	{
		protected readonly IEventEmitter nextEmitter;

		protected ChainedEventEmitter(IEventEmitter nextEmitter)
		{
			if (nextEmitter == null)
			{
				throw new ArgumentNullException("nextEmitter");
			}

			this.nextEmitter = nextEmitter;
		}

		public virtual void Emit(AliasEventInfo eventInfo)
		{
			nextEmitter.Emit(eventInfo);
		}

		public virtual void Emit(ScalarEventInfo eventInfo)
		{
			nextEmitter.Emit(eventInfo);
		}

		public virtual void Emit(MappingStartEventInfo eventInfo)
		{
			nextEmitter.Emit(eventInfo);
		}

		public virtual void Emit(MappingEndEventInfo eventInfo)
		{
			nextEmitter.Emit(eventInfo);
		}

		public virtual void Emit(SequenceStartEventInfo eventInfo)
		{
			nextEmitter.Emit(eventInfo);
		}

		public virtual void Emit(SequenceEndEventInfo eventInfo)
		{
			nextEmitter.Emit(eventInfo);
		}
	}
}