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

		bool IObjectGraphVisitor.Enter(object value, Type type)
		{
			return true;
		}

		bool IObjectGraphVisitor.EnterMapping(object key, Type keyType, object value, Type valueType)
		{
			return true;
		}

		bool IObjectGraphVisitor.EnterMapping(IPropertyDescriptor key, object value)
		{
			return true;
		}

		void IObjectGraphVisitor.VisitScalar(object scalar, Type scalarType)
		{
			eventEmitter.Emit(new ScalarEventInfo(scalar, scalarType));
		}

		void IObjectGraphVisitor.VisitMappingStart(object mapping, Type mappingType, Type type, Type valueType)
		{
			eventEmitter.Emit(new MappingStartEventInfo(mapping, mappingType));
		}

		void IObjectGraphVisitor.VisitMappingEnd(object mapping, Type mappingType)
		{
			eventEmitter.Emit(new MappingEndEventInfo(mapping, mappingType));
		}

		void IObjectGraphVisitor.VisitSequenceStart(object sequence, Type sequenceType, Type elementType)
		{
			eventEmitter.Emit(new SequenceStartEventInfo(sequence, sequenceType));
		}

		void IObjectGraphVisitor.VisitSequenceEnd(object sequence, Type sequenceType)
		{
			eventEmitter.Emit(new SequenceEndEventInfo(sequence, sequenceType));
		}
	}
}