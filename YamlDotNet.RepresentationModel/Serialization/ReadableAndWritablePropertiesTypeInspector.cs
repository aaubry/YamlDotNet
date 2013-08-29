using System;
using System.Collections.Generic;
using System.Linq;

namespace YamlDotNet.RepresentationModel.Serialization
{
	/// <summary>
	/// Returns the properties of a type that are both readable and writable.
	/// </summary>
	public sealed class ReadableAndWritablePropertiesTypeInspector : TypeInspectorSkeleton
	{
		private readonly ITypeInspector _innerTypeDescriptor;

		public ReadableAndWritablePropertiesTypeInspector(ITypeInspector innerTypeDescriptor)
		{
			_innerTypeDescriptor = innerTypeDescriptor;
		}
	
		public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object container)
		{
			return _innerTypeDescriptor.GetProperties(type, container)
				.Where(p => p.CanWrite);
		}
	}
}
