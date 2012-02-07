using System;

namespace YamlDotNet.RepresentationModel.Serialization
{
	public sealed class AnchorAssigningObjectGraphVisitor : ChainedObjectGraphVisitor
	{
		private readonly IEventEmitter eventEmitter;
		private readonly IAliasProvider aliasProvider;

		public AnchorAssigningObjectGraphVisitor(IObjectGraphVisitor nextVisitor, IEventEmitter eventEmitter, IAliasProvider aliasProvider)
			: base(nextVisitor)
		{
			this.eventEmitter = eventEmitter;
			this.aliasProvider = aliasProvider;
		}

		public override bool Enter(object value, Type type)
		{
			if (value != null)
			{
				var alias = aliasProvider.GetAlias(value);
				if (alias != null)
				{
					eventEmitter.Emit(new AliasEventInfo(value, type) {Alias = alias});
					return false;
				}
			}

			return base.Enter(value, type);
		}
	}
}