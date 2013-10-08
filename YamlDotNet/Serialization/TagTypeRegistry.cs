using System;
using System.Collections.Generic;
using System.Reflection;
using YamlDotNet.Schemas;

namespace YamlDotNet.Serialization
{
	/// <summary>
	/// Default implementation of ITagTypeRegistry.
	/// </summary>
	internal class TagTypeRegistry : ITagTypeRegistry
	{
		private readonly IYamlSchema schema;
		private readonly Dictionary<string, Type> tagToType;
		private readonly Dictionary<Type, string> typeToTag;
		private readonly List<Assembly> lookupAssemblies;

		private static readonly List<Assembly> DefaultLookupAssemblies = new List<Assembly>()
			{
				typeof (int).Assembly,
			};

		/// <summary>
		/// Initializes a new instance of the <see cref="TagTypeRegistry"/> class.
		/// </summary>
		public TagTypeRegistry(IYamlSchema schema)
		{
			if (schema == null) throw new ArgumentNullException("schema");
			this.schema = schema;
			tagToType = new Dictionary<string, Type>();
			typeToTag = new Dictionary<Type, string>();
			lookupAssemblies = new List<Assembly>();
		}

		public void RegisterAssembly(Assembly assembly)
		{
			if (assembly == null) throw new ArgumentNullException("assembly");

			// Add automatically the assembly for lookup
			if (!DefaultLookupAssemblies.Contains(assembly) && !lookupAssemblies.Contains(assembly))
			{
				lookupAssemblies.Add(assembly);
			}
		}

		/// <summary>
		/// Register a mapping between a tag and a type.
		/// </summary>
		/// <param name="tag">The tag.</param>
		/// <param name="type">The type.</param>
		public virtual void RegisterTagMapping(string tag, Type type)
		{
			if (tag == null) throw new ArgumentNullException("tag");
			if (type == null) throw new ArgumentNullException("type");

			// Prefix all tags by !
			tag = Uri.EscapeUriString(tag);
			if (tag.StartsWith("tag:"))
			{
				// shorten tag
				// TODO this is not really failsafe
				var shortTag = "!!" + tag.Substring(tag.LastIndexOf(':') + 1);

				// Auto register tag to schema
				schema.RegisterTag(shortTag, tag);
				tag = shortTag;
			}

			tag = tag.StartsWith("!") ? tag : "!" + tag;

			tagToType[tag] = type;
			typeToTag[type] = tag;

			// Make sure the assembly is registered for this type
			RegisterAssembly(type.Assembly);
		}

		public virtual Type TypeFromTag(string tag)
		{
			if (tag == null)
			{
				return null;
			}

			// Get the default schema type if there is any
			var shortTag = schema.ShortenTag(tag);
			Type type;
			if (shortTag != tag)
			{
				type = schema.GetTypeForDefaultTag(shortTag);
				if (type != null)
				{
					return type;
				}
			}

			// un-escape tag
			shortTag = Uri.UnescapeDataString(shortTag);

			// Else try to find a registered alias
			if (tagToType.TryGetValue(shortTag, out type))
			{
				return type;
			}

			// Else resolve type from assembly
			var tagAsType = shortTag.StartsWith("!") ? shortTag.Substring(1) : shortTag;

			// Try to resolve the type from registered assemblies
			type = ResolveType(tagAsType);

			// Register a type that was found
			tagToType.Add(shortTag, type);
			if (type != null)
			{
				typeToTag.Add(type, shortTag);
			}

			return type;
		}

		public virtual string TagFromType(Type type)
		{
			if (type == null)
			{
				return "!!null";
			}

			string tagName;
			// First try to resolve a tag from registered tag
			if (!typeToTag.TryGetValue(type, out tagName))
			{
				// Else try to use schema tag for scalars
				// Else use full name of the type
				tagName = schema.GetDefaultTag(type) ?? string.Format("!{0}", type.FullName);
			}

			return tagName;
		}

		public virtual Type ResolveType(string typeName)
		{
			var type = Type.GetType(typeName);
			if (type == null)
			{
				foreach (var assembly in lookupAssemblies)
				{
					type = assembly.GetType(typeName);
					if (type != null)
					{
						break;
					}
				}
			}
			return type;
		}
	}
}