using System;

namespace YamlDotNet.RepresentationModel.Serialization
{
	public abstract class ChainedObjectGraphVisitor : IObjectGraphVisitor
	{
		private readonly IObjectGraphVisitor nextVisitor;

		protected ChainedObjectGraphVisitor(IObjectGraphVisitor nextVisitor)
		{
			this.nextVisitor = nextVisitor;
		}

		public virtual bool Enter(object value, Type type)
		{
			return nextVisitor.Enter(value, type);
		}

		public virtual bool EnterMapping(object key, Type keyType, object value, Type valueType)
		{
			return nextVisitor.EnterMapping(key, keyType, value, valueType);
		}

		public virtual bool EnterMapping(IPropertyDescriptor key, object value)
		{
			return nextVisitor.EnterMapping(key, value);
		}

		public virtual void VisitScalar(object scalar, Type scalarType)
		{
			nextVisitor.VisitScalar(scalar, scalarType);
		}

		public virtual void VisitMappingStart(object mapping, Type mappingType, Type keyType, Type valueType)
		{
			nextVisitor.VisitMappingStart(mapping, mappingType, keyType, valueType);
		}

		public virtual void VisitMappingEnd(object mapping, Type mappingType)
		{
			nextVisitor.VisitMappingEnd(mapping, mappingType);
		}

		public virtual void VisitSequenceStart(object sequence, Type sequenceType, Type elementType)
		{
			nextVisitor.VisitSequenceStart(sequence, sequenceType, elementType);
		}

		public virtual void VisitSequenceEnd(object sequence, Type sequenceType)
		{
			nextVisitor.VisitSequenceEnd(sequence, sequenceType);
		}
	}
}