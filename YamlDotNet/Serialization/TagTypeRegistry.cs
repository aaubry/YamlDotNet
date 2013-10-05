using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using YamlDotNet.Events;

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

		public Type TypeFromTag(string tagName)
		{
			if (tagName == null)
			{
				return null;
			}

			Type type;
			if (tagToType.TryGetValue(tagName, out type))
			{
				return type;
			}

			type = Type.GetType(tagName);
			if (type == null)
			{
				foreach (var assembly in lookupAssemblies)
				{
					type = assembly.GetType(tagName);
					if (type != null)
					{
						break;
					}
				}
			}
			return type;
		}

		public string TagFromType(Type type)
		{
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