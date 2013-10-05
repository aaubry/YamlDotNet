using System;
using System.Collections.Generic;
using System.Reflection;

namespace YamlDotNet.Serialization
{
	/// <summary>
	/// Prodives tag discovery from a type and type discovery from a tag.
	/// </summary>
	public interface ITagTypeRegistry
	{
		/// <summary>
		/// Finds a type from a tag, null if not found.
		/// </summary>
		/// <param name="tagName">Name of the tag.</param>
		/// <returns>A Type or null if not found</returns>
		Type TypeFromTag(string tagName);

		/// <summary>
		/// Finds a tag from a type, null if not found.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns>A tag or null if not found</returns>
		string TagFromType(Type type);

		/// <summary>
		/// Gets the lookup assembly list used to look for types.
		/// </summary>
		/// <value>The lookup assemblies.</value>
		List<Assembly> LookupAssemblies { get; }
	}
}