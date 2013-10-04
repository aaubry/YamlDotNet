using System;
using System.Collections.Generic;

namespace YamlDotNet.Serialization
{
	/// <summary>
	/// Provides access members of a type.
	/// </summary>
	public interface ITypeDescriptor
	{
		/// <summary>
		/// Gets the type described by this instance.
		/// </summary>
		/// <value>The type.</value>
		Type Type { get; }

		/// <summary>
		/// Gets the members of this type.
		/// </summary>
		/// <value>The members.</value>
		IEnumerable<IMemberDescriptor> Members { get; }

		/// <summary>
		/// Gets the member count.
		/// </summary>
		/// <value>The member count.</value>
		int Count { get; }

		/// <summary>
		/// Gets a value indicating whether this instance has members.
		/// </summary>
		/// <value><c>true</c> if this instance has members; otherwise, <c>false</c>.</value>
		bool HasMembers { get; }

		/// <summary>
		/// Gets the <see cref="IMemberDescriptor"/> with the specified name.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns>The member.</returns>
		IMemberDescriptor this[string name] { get; }

		/// <summary>
		/// Determines whether this instance contains a member with the specified member name.
		/// </summary>
		/// <param name="memberName">Name of the member.</param>
		/// <returns><c>true</c> if this instance contains a member with the specified member name; otherwise, <c>false</c>.</returns>
		bool Contains(string memberName);
	}
}
