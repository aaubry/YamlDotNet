using System;
using System.Linq;
using System.Collections.Generic;
using YamlDotNet.Core;

namespace YamlDotNet.RepresentationModel.Serialization
{
	/// <summary>
	/// Contains additional parameters thatr control the deserialization process.
	/// </summary>
	public sealed class DeserializationOptions
	{
		private readonly DeserializationOverrides overrides;

		/// <summary>
		/// Gets or sets the overrides.
		/// </summary>
		/// <value>The overrides.</value>
		public DeserializationOverrides Overrides
		{
			get
			{
				return overrides;
			}
		}

		private readonly TagMappings mappings;

		/// <summary>
		/// Gets the mappings.
		/// </summary>
		/// <value>The mappings.</value>
		public TagMappings Mappings
		{
			get
			{
				return mappings;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DeserializationOptions"/> class.
		/// </summary>
		public DeserializationOptions()
		{
			overrides = new DeserializationOverrides();
			mappings = new TagMappings();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DeserializationOptions"/> class.
		/// </summary>
		/// <param name="overrides">The overrides.</param>
		/// <param name="mappings">The mappings.</param>
		public DeserializationOptions(IEnumerable<DeserializationOverride> overrides, IDictionary<string, Type> mappings)
		{
			this.overrides = new DeserializationOverrides(overrides);
			this.mappings = new TagMappings(mappings);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DeserializationOptions"/> class.
		/// </summary>
		/// <param name="overrides">The overrides.</param>
		/// <param name="mappings">The mappings.</param>
		[Obsolete("Use DeserializationOptions(IEnumerable<DeserializationOverride> overrides, IDictionary<string, Type> mappings) instead.")]
		public DeserializationOptions(IDictionary<Type, Dictionary<string, Action<object, EventReader>>> overrides, IDictionary<string, Type> mappings)
		{
			var overrideList = from over in overrides
							   from prop in over.Value
							   select new DeserializationOverride(over.Key, prop.Key, prop.Value);

			this.overrides = new DeserializationOverrides(overrideList);
			this.mappings = new TagMappings(mappings);
		}
	}
}