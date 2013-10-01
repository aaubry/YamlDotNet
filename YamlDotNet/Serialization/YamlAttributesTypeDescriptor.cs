using System;
using System.Collections.Generic;
using System.Linq;

namespace YamlDotNet.RepresentationModel.Serialization
{
	/// <summary>
	/// Applies the Yaml* attributes to another <see cref="ITypeDescriptor"/>.
	/// </summary>
	public sealed class YamlAttributesTypeDescriptor : TypeDescriptorSkeleton
	{
		private readonly ITypeDescriptor innerTypeDescriptor;

		public YamlAttributesTypeDescriptor(ITypeDescriptor innerTypeDescriptor)
		{
			this.innerTypeDescriptor = innerTypeDescriptor;
		}

		public override IEnumerable<IPropertyDescriptor> GetProperties(Type type)
		{
			return innerTypeDescriptor.GetProperties(type)
				.Where(p => p.Property.GetCustomAttributes(typeof(YamlIgnoreAttribute), true).Length == 0)
				.Select(p =>
				{
					var alias = (YamlAliasAttribute)p.Property.GetCustomAttributes(typeof(YamlAliasAttribute), true).SingleOrDefault();
					return alias != null ? new PropertyDescriptor(p.Property, alias.Alias) : p;
				});
		}
	}
}
