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

		/// <summary>
		/// Gets or sets a value indicating whether [use short type name].
		/// </summary>
		/// <value><c>true</c> if [use short type name]; otherwise, <c>false</c>.</value>
		public bool UseShortTypeName { get; set; }

		public void RegisterAssembly(Assembly assembly, IAttributeRegistry attributeRegistry)
		{
			if (assembly == null) throw new ArgumentNullException("assembly");
			if (attributeRegistry == null) throw new ArgumentNullException("attributeRegistry");

			// Add automatically the assembly for lookup
			if (!DefaultLookupAssemblies.Contains(assembly) && !lookupAssemblies.Contains(assembly))
			{
				lookupAssemblies.Add(assembly);

				// Register all tags automatically.
				foreach (var type in assembly.GetTypes())
				{
					var tagAttribute = attributeRegistry.GetAttribute<YamlTagAttribute>(type);
					if (tagAttribute != null && !string.IsNullOrEmpty(tagAttribute.Tag))
					{
						RegisterTagMapping(tagAttribute.Tag, type);
					}
				}
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
			if (type != null && !typeToTag.ContainsKey(type))
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
				var typeName = UseShortTypeName ? type.GetShortAssemblyQualifiedName() : type.AssemblyQualifiedName;
				tagName = schema.GetDefaultTag(type) ?? Uri.EscapeUriString(string.Format("!{0}", typeName));
				typeToTag.Add(type, tagName);
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