using System;

namespace YamlDotNet.Serialization
{
	internal class ChainedProcessor : IYamlProcessor
	{
		private readonly IYamlProcessor next;

		public ChainedProcessor(IYamlProcessor next)
		{
			if (next == null) throw new ArgumentNullException("next");
			this.next = next;
		}

		public virtual object ReadYaml(SerializerContext context, object value, Type expectedType)
		{
			return next.ReadYaml(context, value, expectedType);
		}

		public virtual void WriteYaml(SerializerContext context, object value, Type type)
		{
			next.WriteYaml(context, value, type);
		}
	}
}