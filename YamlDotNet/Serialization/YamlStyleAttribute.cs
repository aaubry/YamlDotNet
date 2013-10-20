using System;

namespace YamlDotNet.Serialization
{
	/// <summary>
	/// An attribute to modify the output style of a sequence or mapping. 
	/// This attribute can be apply directly on a type or on a property/field.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property | AttributeTargets.Field)]
	public class YamlStyleAttribute : Attribute
	{
		private readonly YamlStyle style;

		/// <summary>
		/// Initializes a new instance of the <see cref="YamlStyleAttribute"/> class.
		/// </summary>
		/// <param name="style">The style.</param>
		public YamlStyleAttribute(YamlStyle style)
		{
			this.style = style;
		}

		/// <summary>
		/// Gets the style.
		/// </summary>
		/// <value>The style.</value>
		public YamlStyle Style
		{
			get { return style; }
		}
	}
}