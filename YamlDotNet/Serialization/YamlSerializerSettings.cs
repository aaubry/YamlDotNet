using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using YamlDotNet.Schemas;
using YamlDotNet.Serialization.Descriptors;

namespace YamlDotNet.Serialization
{
	/// <summary>
	/// Settings used to configure serialization.
	/// </summary>
    public class YamlSerializerSettings
	{
		private ITagTypeRegistry tagTypeRegistry;
	    private IAttributeRegistry attributeRegistry;
		private IYamlSchema schema;
		private string specialCollectionMember;

		/// <summary>
		/// Initializes a new instance of the <see cref="YamlSerializerSettings"/> class.
		/// </summary>
	    public YamlSerializerSettings()
	    {
		    SortKeyForMapping = true;
		    EmitJsonComptible = false;
		    EmitCapacityForList = false;
			SpecialCollectionMember = "~Items";
			schema = new CoreSchema();
			tagTypeRegistry = new TagTypeRegistry();
			AttributeRegistry = new AttributeRegistry();
	    }

		/// <summary>
		/// Gets or sets a value indicating whether to enable sorting keys for YAML mapping. Default is true. See remarks.
		/// </summary>
		/// <value><c>true</c> to enable sorting keys for YAML mapping; otherwise, <c>false</c>.</value>
		/// <remarks>
		/// When storing a YAML document, It can be important to keep the same order for key mapping in order to keep
		/// a YAML document versionable/diffable.
		/// </remarks>
		public bool SortKeyForMapping { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to emit json comptible YAML.
		/// </summary>
		/// <value><c>true</c> if to emit json comptible YAML; otherwise, <c>false</c>.</value>
	    public bool EmitJsonComptible { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the property <see cref="List{T}.Capacity"/> should be emitted. Default is false.
		/// </summary>
		/// <value><c>true</c> if the property <see cref="List{T}.Capacity"/> should be emitted; otherwise, <c>false</c>.</value>
		public bool EmitCapacityForList { get; set; }

		/// <summary>
		/// Gets or sets the prefix used to serialize items for a non pure <see cref="System.Collections.IDictionary" /> or <see cref="System.Collections.ICollection" />. Default to "~Items", see remarks.
		/// </summary>
		/// <value>The prefix for items.</value>
		/// <remarks>A pure <see cref="System.Collections.IDictionary" /> or <see cref="System.Collections.ICollection" /> is a class that inherits from these types but are not adding any
		/// public properties or fields. When these types are pure, they are respectively serialized as a YAML mapping (for dictionary) or a YAML sequence (for collections).
		/// If the collection type to serialize is not pure, the type is serialized as a YAML mapping sequence that contains the public properties/fields as well as a
		/// special fielx (e.g. "~Items") that contains the actual items of the collection (either a mapping for dictionary or a sequence for collections).
		/// The <see cref="SpecialCollectionMember" /> is this special key that is used when serializing items of a non-pure collection.</remarks>
		public string SpecialCollectionMember
		{
			get { return specialCollectionMember; }
			set
			{
				if (value == null) throw new ArgumentNullException("value");

				// TODO this is a poor check. Need to verify this against the specs
				if (value.Length < 2 || !(value.Contains(".") || value.Contains("~") || value.IndexOf('-') > 0))
				{
					throw new ArgumentException("Expecting length >= 2 and at least a special character '.', '~', '-' (not starting on first char for '-')");
				}

				specialCollectionMember = value;
			}
		}

		/// <summary>
		/// Gets or sets the attribute registry.
		/// </summary>
		/// <value>The attribute registry.</value>
		public IAttributeRegistry AttributeRegistry
		{
			get { return attributeRegistry; }
			set
			{
				if (value == null) throw new ArgumentNullException("value");
				attributeRegistry = value;
			}
		}

		/// <summary>
		/// Gets or sets the tag type registry. Default is <see cref="YamlDotNet.Serialization.TagTypeRegistry"/>
		/// </summary>
		/// <value>The tag type registry.</value>
		/// <exception cref="System.ArgumentNullException">value</exception>
		public ITagTypeRegistry TagTypeRegistry
		{
			get { return tagTypeRegistry; }
			set
			{
				if (value == null) throw new ArgumentNullException("value");
				tagTypeRegistry = value;
			}
		}

		/// <summary>
		/// Gets or sets the schema. Default is <see cref="CoreSchema"/>. When setting the schema in this settings, the schema is initialized by calling its <see cref="IYamlSchema.Initialize"/> method.
		/// </summary>
		/// <value>The schema.</value>
		/// <exception cref="System.ArgumentNullException">value</exception>
		public IYamlSchema Schema
		{
			get { return schema; }
			set
			{
				if (value == null) throw new ArgumentNullException("value");
				schema = value;
			}
		}
	}
}