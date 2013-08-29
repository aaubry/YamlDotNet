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

		public override bool Enter(IObjectDescriptor value)
		{
			var alias = aliasProvider.GetAlias(value.Value);
			if (alias != null && !emittedAliases.Add(alias))
			{
				eventEmitter.Emit(new AliasEventInfo(value) { Alias = alias });
				return false;
			}

			return base.Enter(value);
		}

		public override void VisitMappingStart(IObjectDescriptor mapping, Type keyType, Type valueType)
		{
			eventEmitter.Emit(new MappingStartEventInfo(mapping) { Anchor = aliasProvider.GetAlias(mapping.Value) });
		}

		public override void VisitSequenceStart(IObjectDescriptor sequence, Type elementType)
		{
			eventEmitter.Emit(new SequenceStartEventInfo(sequence) { Anchor = aliasProvider.GetAlias(sequence.Value) });
		}

		public override void VisitScalar(IObjectDescriptor scalar)
		{
			eventEmitter.Emit(new ScalarEventInfo(scalar) { Anchor = aliasProvider.GetAlias(scalar.Value) });
		}
	}
}