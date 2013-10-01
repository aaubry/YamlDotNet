using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace YamlDotNet.RepresentationModel.Serialization
{
	/// <summary>
	/// Wraps another <see cref="ITypeDescriptor"/> and applies a
	/// naming convention to the names of the properties.
	/// </summary>
	public sealed class NamingConventionTypeDescriptor : TypeDescriptorSkeleton
	{
		private readonly ITypeDescriptor innerTypeDescriptor;
		private readonly INamingConvention namingConvention;

		public NamingConventionTypeDescriptor(ITypeDescriptor innerTypeDescriptor, INamingConvention namingConvention)
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

		public override IEnumerable<IPropertyDescriptor> GetProperties(Type type)
		{
			return innerTypeDescriptor.GetProperties(type)
				.Select(p => (IPropertyDescriptor)new PropertyDescriptor(p.Property, namingConvention.Apply(p.Name)));
		}
	}
}
