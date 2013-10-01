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
		protected override bool IsValidProperty (System.Reflection.PropertyInfo property)
		{
			return base.IsValidProperty (property) && property.CanWrite;
		}
	}
}
