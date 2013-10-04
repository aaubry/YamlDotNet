using System;

namespace YamlDotNet.Serialization
{
	/// <summary>
	/// Specify the way to store a property or field of some class or structure.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public sealed class YamlMemberAttribute : Attribute
	{
		private readonly SerializeMemberMode serializeMethod;
		private readonly string name;

		/// <summary>
		/// Initializes a new instance of the <see cref="YamlMemberAttribute"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		public YamlMemberAttribute(string name)
		{
			this.name = name;
			serializeMethod = SerializeMemberMode.Assign;
		}


		/// <summary>
		/// Specify the way to store a property or field of some class or structure.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="serializeMethod">The serialize method.</param>
		public YamlMemberAttribute(string name, SerializeMemberMode serializeMethod)
		{
			this.name = name;
			this.serializeMethod = serializeMethod;
		}

		/// <summary>
		/// Specify the way to store a property or field of some class or structure.
		/// </summary>
		/// <param name="serializeMethod">The serialize method.</param>
		public YamlMemberAttribute(SerializeMemberMode serializeMethod)
		{
			this.serializeMethod = serializeMethod;
		}

		/// <summary>
		/// Gets the name.
		/// </summary>
		/// <value>The name.</value>
		public string Name
		{
			get { return name; }
		}

		/// <summary>
		/// Gets the serialize method1.
		/// </summary>
		/// <value>The serialize method1.</value>
		public SerializeMemberMode SerializeMethod
		{
			get { return serializeMethod; }
		}
	}
}