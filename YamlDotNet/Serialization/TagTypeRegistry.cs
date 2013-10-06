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
		/// Register an alias between a tag and a type.
		/// </summary>
		/// <param name="tag">The tag.</param>
		/// <param name="type">The type.</param>
		public void AddTagAlias(string tag, Type type)
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
			var longTag = schema.ExpandTag(tag);
			Type type;
			if (longTag != tag)
			{
				type = schema.GetTypeForDefaultTag(longTag);
				if (type != null)
				{
					return type;
				}
			}

			// Unescape tag
			longTag = Uri.UnescapeDataString(longTag);

			// Else try to find a registered alias
			if (tagToType.TryGetValue(longTag, out type))
			{
				return type;
			}

			// Else resolve type from assembly
			var tagAsType = longTag.StartsWith("!") ? longTag.Substring(1) : longTag;

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
			tagToType.Add(longTag, type);
			if (type != null)
			{
				typeToTag.Add(type, longTag);
			}

			return type;
		}

		public string TagFromType(IYamlSchema schema, Type type)
		{
			if (schema == null) throw new ArgumentNullException("schema");

			string tagName;
			if (!typeToTag.TryGetValue(type, out tagName))
			{
				return string.Format("!{0}", type.FullName);
			}
			return tagName;
		}

		public List<Assembly> LookupAssemblies
		{
			get { return lookupAssemblies; }
		}
	}
}