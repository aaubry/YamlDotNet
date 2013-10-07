using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using YamlDotNet.Events;
using YamlDotNet.Schemas;

namespace YamlDotNet.Serialization
{
	/// <summary>
	/// Default implementation of ITagTypeRegistry.
	/// </summary>
	public class TagTypeRegistry : ITagTypeRegistry
	{
		private readonly Dictionary<string, Type> tagToType;
		private readonly Dictionary<Type, string> typeToTag;
		private readonly List<Assembly> lookupAssemblies;

		/// <summary>
		/// Initializes a new instance of the <see cref="TagTypeRegistry"/> class.
		/// </summary>
		public TagTypeRegistry()
		{
			tagToType = new Dictionary<string, Type>();
			typeToTag = new Dictionary<Type, string>();
			lookupAssemblies = new List<Assembly>();
		}

		/// <summary>
		/// Register a mapping between a tag and a type.
		/// </summary>
		/// <param name="tag">The tag.</param>
		/// <param name="type">The type.</param>
		public void AddTagMapping(string tag, Type type)
		{
			if (tag == null) throw new ArgumentNullException("tag");
			if (type == null) throw new ArgumentNullException("type");

			// Prefix all tags by !
			tag = Uri.EscapeUriString(tag);
			tag = tag.StartsWith("!") ? tag : "!" + tag;

			tagToType[tag] = type;
			typeToTag[type] = tag;

			// Add automatically the assembly for lookup
			if (!lookupAssemblies.Contains(type.Assembly))
			{
				lookupAssemblies.Add(type.Assembly);
			}
		}

		public Type TypeFromTag(IYamlSchema schema, string tag)
		{
			if (schema == null) throw new ArgumentNullException("schema");

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

			type = Type.GetType(tagAsType);
			if (type == null)
			{
				foreach (var assembly in lookupAssemblies)
				{
					type = assembly.GetType(tagAsType);
					if (type != null)
					{
						break;
					}
				}
			}

			// Register a type that was found
			tagToType.Add(shortTag, type);
			if (type != null)
			{
				typeToTag.Add(type, shortTag);
			}

			return type;
		}

		public string TagFromType(IYamlSchema schema, Type type)
		{
			if (schema == null) throw new ArgumentNullException("schema");

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

		public List<Assembly> LookupAssemblies
		{
			get { return lookupAssemblies; }
		}
	}
}