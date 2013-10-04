using System;

namespace YamlDotNet.Serialization
{
	/// <summary>
	/// An attribute to associate a tag with a particular type.
	/// </summary>
	[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Enum)]
	public class YamlTagAttribute : Attribute
	{
		private readonly string tag;

		/// <summary>
		/// Initializes a new instance of the <see cref="YamlTagAttribute"/> class.
		/// </summary>
		/// <param name="tag">The tag.</param>
		public YamlTagAttribute(string tag)
		{
			this.tag = tag;
		}

		/// <summary>
		/// Gets the tag.
		/// </summary>
		/// <value>The tag.</value>
		public string Tag { get { return tag; } }
	}
}