using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace YamlDotNet.RepresentationModel.Serialization.ObjectGraphVisitors
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

		private static readonly IEqualityComparer<object> _objectComparer = EqualityComparer<object>.Default;

		public override bool EnterMapping(IObjectDescriptor key, IObjectDescriptor value)
		{
			return !_objectComparer.Equals(value, GetDefault(value.Type))
			       && base.EnterMapping(key, value);
		}

		public override bool EnterMapping(IPropertyDescriptor key, object value)
		{
			var defaultValueAttribute = key.GetCustomAttribute<DefaultValueAttribute>();
			var defaultValue = defaultValueAttribute != null
				? defaultValueAttribute.Value
				: GetDefault(key.Type);

			return !_objectComparer.Equals(value, defaultValue)
				   && base.EnterMapping(key, value);
		}
	}
}