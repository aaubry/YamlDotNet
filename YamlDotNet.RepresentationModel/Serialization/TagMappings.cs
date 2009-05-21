using System;
using System.Collections.Generic;

namespace YamlDotNet.RepresentationModel.Serialization
{
	/// <summary>
	/// Contains mappings between tags and types.
	/// </summary>
	public sealed class TagMappings
	{
		private readonly IDictionary<string, Type> mappings;

		/// <summary>
		/// Initializes a new instance of the <see cref="TagMappings"/> class.
		/// </summary>
		public TagMappings()
		{
			mappings = new Dictionary<string, Type>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TagMappings"/> class.
		/// </summary>
		/// <param name="mappings">The mappings.</param>
		public TagMappings(IDictionary<string, Type> mappings)
		{
			this.mappings = new Dictionary<string, Type>(mappings);
		}

		/// <summary>
		/// Adds the specified tag.
		/// </summary>
		/// <param name="tag">The tag.</param>
		/// <param name="mapping">The mapping.</param>
		public void Add(string tag, Type mapping)
		{
			mappings.Add(tag, mapping);
		}

		/// <summary>
		/// Gets the mapping.
		/// </summary>
		/// <param name="tag">The tag.</param>
		/// <returns></returns>
		internal Type GetMapping(string tag)
		{
			Type mapping;
			if (mappings.TryGetValue(tag, out mapping))
			{
				return mapping;
			}
			else
			{
				return null;
			}
		}
	}
}