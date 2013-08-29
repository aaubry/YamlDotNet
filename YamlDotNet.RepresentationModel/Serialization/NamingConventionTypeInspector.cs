using System;
using System.Collections.Generic;
using System.Linq;

namespace YamlDotNet.RepresentationModel.Serialization
{
	/// <summary>
	/// Wraps another <see cref="ITypeInspector"/> and applies a
	/// naming convention to the names of the properties.
	/// </summary>
	public sealed class NamingConventionTypeInspector : TypeInspectorSkeleton
	{
		private readonly ITypeInspector innerTypeDescriptor;
		private readonly INamingConvention namingConvention;

		public NamingConventionTypeInspector(ITypeInspector innerTypeDescriptor, INamingConvention namingConvention)
		{
			if (innerTypeDescriptor == null)
			{
				throw new ArgumentNullException("innerTypeDescriptor");
			}

			this.innerTypeDescriptor = innerTypeDescriptor;

			if (namingConvention == null)
			{
				throw new ArgumentNullException("namingConvention");
			}

			this.namingConvention = namingConvention;
		}

		public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object container)
		{
			return innerTypeDescriptor.GetProperties(type, container)
				.Select(p => (IPropertyDescriptor)new PropertyDescriptor(p) { Name = namingConvention.Apply(p.Name) });
		}
	}
}
