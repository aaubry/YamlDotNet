using System;
using System.Collections.Generic;
using System.Reflection;

namespace YamlDotNet.Serialization
{
	/// <summary>
	/// A registry for all attributes.
	/// </summary>
	public interface IAttributeRegistry
	{
		/// <summary>
		/// Gets the attributes associated with the specified member.
		/// </summary>
		/// <param name="memberInfo">The reflection member.</param>
		/// <param name="inherit">if set to <c>true</c> includes inherited attributes.</param>
		/// <returns>An enumeration of <see cref="Attribute"/>.</returns>
		List<Attribute> GetAttributes(MemberInfo memberInfo, bool inherit = true);

		/// <summary>
		/// Gets the attributes associated with the specified member.
		/// </summary>
		/// <typeparam name="T">Type of the attribute</typeparam>
		/// <param name="memberInfo">The member information.</param>
		/// <param name="inherit">if set to <c>true</c> [inherit].</param>
		/// <returns>An enumeration of <see cref="Attribute"/>.</returns>
		IEnumerable<T> GetAttributes<T>(MemberInfo memberInfo, bool inherit = true) where T : Attribute;

		/// <summary>
		/// Gets an attribute associated with the specified member.
		/// </summary>
		/// <typeparam name="T">Type of the attribute</typeparam>
		/// <param name="memberInfo">The member information.</param>
		/// <param name="inherit">if set to <c>true</c> [inherit].</param>
		/// <returns>An attribute of type {T} if it was found; otherwise <c>null</c> </returns>
		T GetAttribute<T>(MemberInfo memberInfo, bool inherit = true) where T : Attribute;

		/// <summary>
		/// Registers an attribute for the specified member. Restriction: Attributes registered this way cannot be listed in inherited attributes.
		/// </summary>
		/// <param name="memberInfo">The member information.</param>
		/// <param name="attribute">The attribute.</param>
		void Register(MemberInfo memberInfo, Attribute attribute);
	}
}