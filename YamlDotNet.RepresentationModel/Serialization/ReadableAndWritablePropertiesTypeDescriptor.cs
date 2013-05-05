using System;
using System.Collections.Generic;
using System.Linq;

namespace YamlDotNet.RepresentationModel.Serialization
{
	/// <summary>
	/// Returns the properties of a type that are both readable and writable.
	/// </summary>
	public sealed class ReadableAndWritablePropertiesTypeDescriptor : ReadablePropertiesTypeDescriptor
	{
		public override IEnumerable<IPropertyDescriptor> GetProperties(Type type)
		{
			return base.GetProperties(type)
				.Where(p => p.Property.CanWrite);
		}
	}
}
