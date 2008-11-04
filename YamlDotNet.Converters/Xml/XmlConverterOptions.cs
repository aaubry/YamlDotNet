using System;

namespace YamlDotNet.Converters.Xml
{
	/// <summary>
	/// Specifies how a <see cref="XmlConverter"/> behaves. 
	/// </summary>
	public class XmlConverterOptions
	{
		private string rootElementName = "root";
		
		/// <value>
		/// Gets the name of the root XML element.
		/// </value>
		public string RootElementName {
			get {
				return rootElementName;
			}
			set {
				if(string.IsNullOrEmpty(value)) {
					throw new ArgumentNullException("value");
				}				
				rootElementName = value;
			}
		}

		private string sequenceElementName = "sequence";
		
		/// <value>
		/// Gets the name of sequence elements.
		/// </value>
		public string SequenceElementName {
			get {
				return sequenceElementName;
			}
			set {
				if(string.IsNullOrEmpty(value)) {
					throw new ArgumentNullException("value");
				}				
				sequenceElementName = value;
			}
		}

		private string mappingElementName = "mapping";
		
		/// <value>
		/// Gets the name of mapping elements.
		/// </value>
		public string MappingElementName {
			get {
				return mappingElementName;
			}
			set {
				if(string.IsNullOrEmpty(value)) {
					throw new ArgumentNullException("value");
				}				
				mappingElementName = value;
			}
		}

		private string mappingKeyElementName = "key";
		
		/// <value>
		/// Gets the name of key elements.
		/// </value>
		public string MappingKeyElementName {
			get {
				return mappingKeyElementName;
			}
			set {
				if(string.IsNullOrEmpty(value)) {
					throw new ArgumentNullException("value");
				}				
				mappingKeyElementName = value;
			}
		}

		private string mappingValueElementName = "value";
		
		/// <value>
		/// Gets the name of key elements.
		/// </value>
		public string MappingValueElementName {
			get {
				return mappingValueElementName;
			}
			set {
				if(string.IsNullOrEmpty(value)) {
					throw new ArgumentNullException("value");
				}				
				mappingValueElementName = value;
			}
		}
		
		
	}
}