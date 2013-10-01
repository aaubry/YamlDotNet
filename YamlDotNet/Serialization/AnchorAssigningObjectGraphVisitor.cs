using System;
using System.Collections.Generic;

namespace YamlDotNet.RepresentationModel.Serialization
{
	public sealed class AnchorAssigningObjectGraphVisitor : ChainedObjectGraphVisitor
	{
		private readonly IEventEmitter eventEmitter;
		private readonly IAliasProvider aliasProvider;
		private readonly HashSet<string> emittedAliases = new HashSet<string>();

		public AnchorAssigningObjectGraphVisitor(IObjectGraphVisitor nextVisitor, IEventEmitter eventEmitter, IAliasProvider aliasProvider)
			: base(nextVisitor)
		{
			this.eventEmitter = eventEmitter;
			this.aliasProvider = aliasProvider;
		}

		public override bool Enter(object value, Type type)
		{
			var alias = aliasProvider.GetAlias(value);
			if (alias != null && !emittedAliases.Add(alias))
			{
				eventEmitter.Emit(new AliasEventInfo(value, type) { Alias = alias });
				return false;
			}

			return base.Enter(value, type);
		}

		public override void VisitMappingStart(object mapping, Type mappingType, Type keyType, Type valueType)
		{
			eventEmitter.Emit(new MappingStartEventInfo(mapping, mappingType) { Anchor = aliasProvider.GetAlias(mapping) });
		}

		public override void VisitSequenceStart(object sequence, Type sequenceType, Type elementType)
		{
			eventEmitter.Emit(new SequenceStartEventInfo(sequence, sequenceType) { Anchor = aliasProvider.GetAlias(sequence) });
		}

		public override void VisitScalar(object scalar, Type scalarType)
		{
			eventEmitter.Emit(new ScalarEventInfo(scalar, scalarType) { Anchor = aliasProvider.GetAlias(scalar) });
		}
	}
}