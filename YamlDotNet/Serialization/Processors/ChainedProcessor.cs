using System;

namespace YamlDotNet.Serialization.Processors
{
	public class ChainedProcessor : IYamlProcessor
	{
		private readonly IYamlProcessor next;

		public ChainedProcessor(IYamlProcessor next)
		{
			if (next == null) throw new ArgumentNullException("next");
			this.next = next;
		}

		public virtual object ReadYaml(SerializerContext context, object value, ITypeDescriptor typeDescriptor)
		{
			return next.ReadYaml(context, value, typeDescriptor);
		}

		public virtual void WriteYaml(SerializerContext context, object value, ITypeDescriptor typeDescriptor)
		{
			next.WriteYaml(context, value, typeDescriptor);
		}
	}
}