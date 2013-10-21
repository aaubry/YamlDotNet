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
		/// <param name="order">The order.</param>
		public YamlMemberAttribute(int order)
		{
			Order = order;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="YamlMemberAttribute"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		public YamlMemberAttribute(string name)
		{
			this.name = name;
			Order = -1;
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
			Order = -1;
		}

		/// <summary>
		/// Specify the way to store a property or field of some class or structure.
		/// </summary>
		/// <param name="serializeMethod">The serialize method.</param>
		public YamlMemberAttribute(SerializeMemberMode serializeMethod)
		{
			this.serializeMethod = serializeMethod;
			Order = -1;
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


		/// <summary>
		/// Gets or sets the order. Default is -1 (default to alphabetical)
		/// </summary>
		/// <value>The order.</value>
		public int Order { get; set; }
	}
}