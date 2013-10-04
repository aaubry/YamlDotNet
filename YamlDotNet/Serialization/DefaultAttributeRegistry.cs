using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace YamlDotNet.Serialization
{
	/// <summary>
	/// A default implementation for <see cref="IAttributeRegistry"/>. 
	/// This implementation allows to retrieve default attributes for a member or 
	/// to attach an attribute to a specific type/member.
	/// </summary>
	public class DefaultAttributeRegistry : IAttributeRegistry
	{
		private readonly Dictionary<MemberInfoKey, List<Attribute>> cachedAttributes = new Dictionary<MemberInfoKey, List<Attribute>>();
		private readonly Dictionary<MemberInfo, List<Attribute>> registeredAttributes = new Dictionary<MemberInfo, List<Attribute>>();

		/// <summary>
		/// Gets the attributes associated with the specified member.
		/// </summary>
		/// <param name="memberInfo">The reflection member.</param>
		/// <param name="inherit">if set to <c>true</c> includes inherited attributes.</param>
		/// <returns>An enumeration of <see cref="Attribute"/>.</returns>
		public List<Attribute> GetAttributes(MemberInfo memberInfo, bool inherit = true)
		{
			var key = new MemberInfoKey(memberInfo, inherit);

			// Use a cache of attributes
			List<Attribute> attributes;
			if (cachedAttributes.TryGetValue(key, out attributes))
			{
				return attributes;
			}

			// Else retrieve all default attributes
			var defaultAttributes = memberInfo.GetCustomAttributes(inherit);
			attributes = defaultAttributes.Cast<Attribute>().ToList();

			// And add registered attributes
			List<Attribute> registered;
			if (registeredAttributes.TryGetValue(memberInfo, out registered))
			{
				attributes.AddRange(registered);
			}

			// Add to the cache
			cachedAttributes.Add(key, attributes);

			return attributes;
		}

		/// <summary>
		/// Gets the attributes associated with the specified member.
		/// </summary>
		/// <typeparam name="T">Type of the attribute</typeparam>
		/// <param name="memberInfo">The member information.</param>
		/// <param name="inherit">if set to <c>true</c> [inherit].</param>
		/// <returns>An enumeration of <see cref="Attribute"/>.</returns>
		public IEnumerable<T> GetAttributes<T>(MemberInfo memberInfo, bool inherit = true) where T : Attribute
		{
			return GetAttributes(memberInfo, inherit).OfType<T>();
		}

		/// <summary>
		/// Gets an attribute associated with the specified member.
		/// </summary>
		/// <typeparam name="T">Type of the attribute</typeparam>
		/// <param name="memberInfo">The member information.</param>
		/// <param name="inherit">if set to <c>true</c> [inherit].</param>
		/// <returns>An attribute of type {T} if it was found; otherwise <c>null</c> </returns>
		public T GetAttribute<T>(MemberInfo memberInfo, bool inherit = true) where T : Attribute
		{
			var list = GetAttributes(memberInfo, inherit);
			if (list.Count > 0)
			{
				return list[list.Count - 1] as T;
			}
			return null;
		}

		/// <summary>
		/// Registers an attribute for the specified member. Restriction: Attributes registered this way cannot be listed in inherited attributes.
		/// </summary>
		/// <param name="memberInfo">The member information.</param>
		/// <param name="attribute">The attribute.</param>
		public void Register(MemberInfo memberInfo, Attribute attribute)
		{
			List<Attribute> attributes;
			if (!registeredAttributes.TryGetValue(memberInfo, out attributes))
			{
				attributes = new List<Attribute>();
				registeredAttributes.Add(memberInfo, attributes);
			}

			attributes.Add(attribute);
		}

		private struct MemberInfoKey : IEquatable<MemberInfoKey>
		{
			private readonly MemberInfo memberInfo;

			private readonly bool inherit;

			public MemberInfoKey(MemberInfo memberInfo, bool inherit)
			{
				this.memberInfo = memberInfo;
				this.inherit = inherit;
			}

			public bool Equals(MemberInfoKey other)
			{
				return memberInfo.Equals(other.memberInfo) && inherit.Equals(other.inherit);
			}

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj)) return false;
				return obj is MemberInfoKey && Equals((MemberInfoKey) obj);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					return (memberInfo.GetHashCode()*397) ^ inherit.GetHashCode();
				}
			}
		}
	}
}