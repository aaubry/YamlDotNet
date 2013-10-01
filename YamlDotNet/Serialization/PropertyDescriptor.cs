using System;
using System.Reflection;

namespace YamlDotNet.RepresentationModel.Serialization
{
	public sealed class PropertyDescriptor : IPropertyDescriptor
	{
		public PropertyDescriptor(PropertyInfo property, string name)
		{
			if (property == null)
			{
				throw new ArgumentNullException("property");
			}

			Property = property;

			if (name == null)
			{
				throw new ArgumentNullException("name");
			}

			Name = name;
		}

		public PropertyInfo Property { get; private set; }
		public string Name { get; private set; }
	}
}
