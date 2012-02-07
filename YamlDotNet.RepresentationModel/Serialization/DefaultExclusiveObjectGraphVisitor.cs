using System;

namespace YamlDotNet.RepresentationModel.Serialization
{
	public sealed class DefaultExclusiveObjectGraphVisitor : ChainedObjectGraphVisitor
	{
		public DefaultExclusiveObjectGraphVisitor(IObjectGraphVisitor nextVisitor)
			: base(nextVisitor)
		{
		}

		private static object GetDefault(Type type)
		{
			return type.IsValueType ? Activator.CreateInstance(type) : null;
		}

		public override bool Enter(object value, Type type)
		{
			return value != GetDefault(type)
			       && base.Enter(value, type);
		}

		public override bool EnterMapping(object key, Type keyType, object value, Type valueType)
		{
			return value != GetDefault(valueType)
			       && base.EnterMapping(key, keyType, value, valueType);
		}
	}
}