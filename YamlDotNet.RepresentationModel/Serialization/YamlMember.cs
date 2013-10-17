using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YamlDotNet.RepresentationModel.Serialization
{
	/// <summary>
	/// Provides special Yaml serialization instructions.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public sealed class YamlMemberAttribute : Attribute
	{
		/// <summary>
		/// Specifies that this property should be serialized as the given type, rather than using the actual runtime value's type.
		/// </summary>
		public Type SerializeAs { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="YamlMemberAttribute" /> class.
		/// </summary>
		/// <param name="serializeAs">Specifies that this property should be serialized as the given type, rather than using the actual runtime value's type.</param>
		public YamlMemberAttribute(Type serializeAs)
		{
			SerializeAs = serializeAs;
		}
	}
}
