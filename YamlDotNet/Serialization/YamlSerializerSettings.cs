using System;
using System.Collections.Generic;
using System.Reflection;

namespace YamlDotNet.Serialization
{
	/// <summary>
	/// Settings used to configure serialization.
	/// </summary>
    public class YamlSerializerSettings
	{
		private ITagTypeRegistry tagTypeRegistry;
	    private IAttributeRegistry attributeRegistry;
	    private ITypeDescriptorFactory typeDescriptorFactory;
		private string prefixForItems;

		/// <summary>
		/// Initializes a new instance of the <see cref="YamlSerializerSettings"/> class.
		/// </summary>
	    public YamlSerializerSettings()
	    {
		    SortKeyForMapping = true;
		    EmitJsonComptible = false;
		    EmitCapacityForList = false;
			PrefixForItems = "~Items";
			tagTypeRegistry = new TagTypeRegistry();
			AttributeRegistry = new AttributeRegistry();
			TypeDescriptorFactory = new DescriptorFactory(this);
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
		/// Gets or sets a value indicating whether the property <see cref="System.Collections.IList.Capacity"/> should be emitted. Default is false.
		/// </summary>
		/// <value><c>true</c> if the property <see cref="System.Collections.IList.Capacity"/> should be emitted; otherwise, <c>false</c>.</value>
		public bool EmitCapacityForList { get; set; }

		public string PrefixForItems
		{
			get { return prefixForItems; }
			set
			{
				// TODO check prefix for items
				prefixForItems = value;
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
		/// Gets or sets the type descriptor factory used when trying to find a <see cref="ITypeDescriptor"/>.
		/// </summary>
		/// <value>The type descriptor factory.</value>
		/// <exception cref="System.ArgumentNullException">value</exception>
		public ITypeDescriptorFactory TypeDescriptorFactory
		{
			get { return typeDescriptorFactory; }
			set
			{
				if (value == null) throw new ArgumentNullException("value");
				typeDescriptorFactory = value;
			}
		}

		/// <summary>
		/// Gets or sets the tag type registry.
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
    }
}