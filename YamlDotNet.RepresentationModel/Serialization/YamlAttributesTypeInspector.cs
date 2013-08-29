using System;
using System.Collections.Generic;
using System.Linq;

namespace YamlDotNet.RepresentationModel.Serialization
{
	/// <summary>
	/// Applies the Yaml* attributes to another <see cref="ITypeInspector"/>.
	/// </summary>
	public sealed class YamlAttributesTypeInspector : TypeInspectorSkeleton
	{
		private readonly ITypeInspector innerTypeDescriptor;

		public YamlAttributesTypeInspector(ITypeInspector innerTypeDescriptor)
		{
			this.innerTypeDescriptor = innerTypeDescriptor;
		}

		public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object container)
		{
			return innerTypeDescriptor.GetProperties(type, container)
				.Where(p => p.GetCustomAttribute<YamlIgnoreAttribute>() == null)
				.Select(p =>
				{
					var descriptor = new PropertyDescriptor(p);

					var alias = p.GetCustomAttribute<YamlAliasAttribute>();
					if (alias != null)
					{
						descriptor.Name = alias.Alias;
					}

					var member = p.GetCustomAttribute<YamlMemberAttribute>();
					if (member != null)
					{
						if (member.SerializeAs != null)
						{
							descriptor.Type = member.SerializeAs;
						}
					}

					return (IPropertyDescriptor)descriptor;
				});
		}
	}
}
