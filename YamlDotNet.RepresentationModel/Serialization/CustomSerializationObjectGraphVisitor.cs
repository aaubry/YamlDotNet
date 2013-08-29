using System;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Core;

namespace YamlDotNet.RepresentationModel.Serialization
{
	public sealed class CustomSerializationObjectGraphVisitor : ChainedObjectGraphVisitor
	{
		private readonly IEmitter emitter;
		private readonly IEnumerable<IYamlTypeConverter> typeConverters;

		public CustomSerializationObjectGraphVisitor(IEmitter emitter, IObjectGraphVisitor nextVisitor, IEnumerable<IYamlTypeConverter> typeConverters)
			: base(nextVisitor)
		{
			this.emitter = emitter;
			this.typeConverters = typeConverters != null
				? typeConverters.ToList()
				: Enumerable.Empty<IYamlTypeConverter>();
		}

		public override bool Enter(IObjectDescriptor value)
		{
			var typeConverter = typeConverters.FirstOrDefault(t => t.Accepts(value.Type));
			if (typeConverter != null)
			{
				typeConverter.WriteYaml(emitter, value.Value, value.Type);
				return false;
			}

			var serializable = value as IYamlSerializable;
			if (serializable != null)
			{
				serializable.WriteYaml(emitter);
				return false;
			}

			return base.Enter(value);
		}
	}
}
