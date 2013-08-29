using System;

namespace YamlDotNet.RepresentationModel.Serialization
{
	public sealed class EmittingObjectGraphVisitor : IObjectGraphVisitor
	{
		private readonly IEventEmitter eventEmitter;

		public EmittingObjectGraphVisitor(IEventEmitter eventEmitter)
		{
			this.eventEmitter = eventEmitter;
		}

		bool IObjectGraphVisitor.Enter(IObjectDescriptor value)
		{
			return true;
		}

		bool IObjectGraphVisitor.EnterMapping(IObjectDescriptor key, IObjectDescriptor value)
		{
			return true;
		}

		bool IObjectGraphVisitor.EnterMapping(IPropertyDescriptor key, object value)
		{
			return true;
		}

		void IObjectGraphVisitor.VisitScalar(IObjectDescriptor scalar)
		{
			eventEmitter.Emit(new ScalarEventInfo(scalar));
		}

		void IObjectGraphVisitor.VisitMappingStart(IObjectDescriptor mapping, Type keyType, Type valueType)
		{
			eventEmitter.Emit(new MappingStartEventInfo(mapping));
		}

		void IObjectGraphVisitor.VisitMappingEnd(IObjectDescriptor mapping)
		{
			eventEmitter.Emit(new MappingEndEventInfo(mapping));
		}

		void IObjectGraphVisitor.VisitSequenceStart(IObjectDescriptor sequence, Type elementType)
		{
			eventEmitter.Emit(new SequenceStartEventInfo(sequence));
		}

		void IObjectGraphVisitor.VisitSequenceEnd(IObjectDescriptor sequence)
		{
			eventEmitter.Emit(new SequenceEndEventInfo(sequence));
		}
	}
}