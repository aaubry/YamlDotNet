using System;

namespace YamlDotNet.Converters.Xml
{
	/// <summary>
	/// Specifies the behavior of the <see cref="XmlConverter"/>.
	/// </summary>
	public class XmlConverterOptions
	{
		private bool isReadOnly;

		/// <summary>
		/// Gets a value indicating whether this instance is readonly.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is readonly; otherwise, <c>false</c>.
		/// </value>
		public bool IsReadOnly
		{
			get
			{
				return isReadOnly;
			}
		}

		private void EnsureIsNotReadonly()
		{
			if(isReadOnly)
			{
				throw new InvalidOperationException("This object cannot be modified.");
			}
		}

		private string rootElementName = "root";

		/// <summary>
		/// Gets or sets the name of the root XML element.
		/// </summary>
		/// <value>The name of the root element.</value>
		public string RootElementName
		{
			get
			{
				return rootElementName;
			}
			set
			{
				EnsureIsNotReadonly();
				if (string.IsNullOrEmpty(value))
				{
					throw new ArgumentNullException("value");
				}

				rootElementName = value;
			}
		}

		private string sequenceElementName = "sequence";

		/// <summary>
		/// Gets or sets the name of the sequence XML element.
		/// </summary>
		/// <value>The name of the sequence element.</value>
		public string SequenceElementName
		{
			get
			{
				return sequenceElementName;
			}
			set
			{
				EnsureIsNotReadonly();
				if (string.IsNullOrEmpty(value))
				{
					throw new ArgumentNullException("value");
				}
				sequenceElementName = value;
			}
		}

		private string sequenceItemElementName = "item";

		/// <summary>
		/// Gets or sets the name of the sequence item XML element.
		/// </summary>
		/// <value>The name of the sequence item element.</value>
		public string SequenceItemElementName
		{
			get
			{
				return sequenceItemElementName;
			}
			set
			{
				EnsureIsNotReadonly();
				if (string.IsNullOrEmpty(value))
				{
					throw new ArgumentNullException("value");
				}
				sequenceItemElementName = value;
			}
		}

		private string mappingElementName = "mapping";

		/// <summary>
		/// Gets or sets the name of the mapping XML element.
		/// </summary>
		/// <value>The name of the mapping element.</value>
		public string MappingElementName
		{
			get
			{
				return mappingElementName;
			}
			set
			{
				EnsureIsNotReadonly();
				if (string.IsNullOrEmpty(value))
				{
					throw new ArgumentNullException("value");
				}
				mappingElementName = value;
			}
		}

		private string mappingEntryElementName = "entry";

		/// <summary>
		/// Gets or sets the name of the mapping entry XML element.
		/// </summary>
		/// <value>The name of the mapping entry element.</value>
		public string MappingEntryElementName
		{
			get
			{
				return mappingEntryElementName;
			}
			set
			{
				EnsureIsNotReadonly();
				if (string.IsNullOrEmpty(value))
				{
					throw new ArgumentNullException("value");
				}
				mappingEntryElementName = value;
			}
		}

		private string mappingKeyElementName = "key";

		/// <summary>
		/// Gets or sets the name of the mapping key XML element.
		/// </summary>
		/// <value>The name of the mapping key element.</value>
		public string MappingKeyElementName
		{
			get
			{
				return mappingKeyElementName;
			}
			set
			{
				EnsureIsNotReadonly();
				if (string.IsNullOrEmpty(value))
				{
					throw new ArgumentNullException("value");
				}
				mappingKeyElementName = value;
			}
		}

		private string mappingValueElementName = "value";

		/// <summary>
		/// Gets or sets the name of the mapping value XML element.
		/// </summary>
		/// <value>The name of the mapping value element.</value>
		public string MappingValueElementName
		{
			get
			{
				return mappingValueElementName;
			}
			set
			{
				EnsureIsNotReadonly();
				if (string.IsNullOrEmpty(value))
				{
					throw new ArgumentNullException("value");
				}
				mappingValueElementName = value;
			}
		}

		/// <summary>
		/// Gets a read-only copy of the current object.
		/// </summary>
		/// <returns></returns>
		public XmlConverterOptions AsReadOnly()
		{
			XmlConverterOptions copy = (XmlConverterOptions)MemberwiseClone();
			copy.isReadOnly = true;
			return copy;
		}

		/// <summary>
		/// The default options.
		/// </summary>
		public static readonly XmlConverterOptions Default = new XmlConverterOptions().AsReadOnly();
	}
}