using System;
using System.Reflection;

namespace YamlDotNet.Serialization
{
	/// <summary>
	/// Prodives tag discovery from a type and type discovery from a tag.
	/// </summary>
	internal interface ITagTypeRegistry
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
		/// Resolves a type from the specified typeName.
		/// </summary>
		/// <param name="typeName">Name of the type.</param>
		/// <returns>Type found for this typeName</returns>
		Type ResolveType(string typeName);

		/// <summary>
		/// Registers an assembly when trying to resolve types.
		/// </summary>
		/// <param name="assembly">The assembly.</param>
		void RegisterAssembly(Assembly assembly);

		/// <summary>
		/// Register a mapping between a tag and a type.
		/// </summary>
		/// <param name="tag">The tag.</param>
		/// <param name="type">The type.</param>
		void RegisterTagMapping(string tag, Type type);
	}
}