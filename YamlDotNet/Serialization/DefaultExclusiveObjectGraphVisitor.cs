using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

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

		private static readonly IEqualityComparer<object> _objectComparer = EqualityComparer<object>.Default;

		public override bool Enter(object value, Type type)
		{
			return base.Enter(value, type);
		}

		public override bool EnterMapping(object key, Type keyType, object value, Type valueType)
		{
			return !_objectComparer.Equals(value, GetDefault(valueType))
			       && base.EnterMapping(key, keyType, value, valueType);
		}

		public override bool EnterMapping(IPropertyDescriptor key, object value)
		{
			var defaultValueAttribute = (DefaultValueAttribute)key.Property.GetCustomAttributes(typeof(DefaultValueAttribute), true).FirstOrDefault();
			var defaultValue = defaultValueAttribute != null
				? defaultValueAttribute.Value
				: GetDefault(key.Property.PropertyType);

			return !_objectComparer.Equals(value, defaultValue)
				   && base.EnterMapping(key, value);
		}
	}
}