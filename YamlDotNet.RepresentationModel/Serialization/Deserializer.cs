//  This file is part of YamlDotNet - A .NET library for YAML.
//  Copyright (c) 2008, 2009, 2010, 2011, 2012, 2013 Antoine Aubry

//  Permission is hereby granted, free of charge, to any person obtaining a copy of
//  this software and associated documentation files (the "Software"), to deal in
//  the Software without restriction, including without limitation the rights to
//  use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
//  of the Software, and to permit persons to whom the Software is furnished to do
//  so, subject to the following conditions:

//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.

//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.

using System;
using System.Collections.Generic;
using YamlDotNet.RepresentationModel.Serialization.NodeDeserializers;
using YamlDotNet.RepresentationModel.Serialization.NodeTypeResolvers;
using YamlDotNet.RepresentationModel.Serialization.NamingConventions;

namespace YamlDotNet.RepresentationModel.Serialization
{
	/// <summary>
	/// A façade for the YAML library with the standard configuration.
	/// </summary>
	public class Deserializer : BareDeserializer
	{
		private static readonly Dictionary<string, Type> predefinedTagMappings = new Dictionary<string, Type>
		{
			{ "tag:yaml.org,2002:map", typeof(Dictionary<object, object>) },
			{ "tag:yaml.org,2002:bool", typeof(bool) },
			{ "tag:yaml.org,2002:float", typeof(double) },
			{ "tag:yaml.org,2002:int", typeof(int) },
			{ "tag:yaml.org,2002:str", typeof(string) },
			{ "tag:yaml.org,2002:timestamp", typeof(DateTime) },
		};

		private readonly Dictionary<string, Type> tagMappings;
		private readonly List<IYamlTypeConverter> converters;
		private TypeDescriptorProxy typeDescriptor = new TypeDescriptorProxy();

		public IList<INodeDeserializer> Deserializers { get; private set; }
		public IList<INodeTypeResolver> TypeResolvers { get; private set; }

		private class TypeDescriptorProxy : ITypeDescriptor
		{
			public ITypeDescriptor TypeDescriptor;
			
			public IEnumerable<IPropertyDescriptor> GetProperties(Type type)
			{
				return TypeDescriptor.GetProperties(type);
			}

			public IPropertyDescriptor GetProperty(Type type, string name)
			{
				return TypeDescriptor.GetProperty(type, name);
			}
		}
		
		public Deserializer(IObjectFactory objectFactory = null, INamingConvention namingConvention = null)
		{
			objectFactory = objectFactory ?? new DefaultObjectFactory();
			namingConvention = namingConvention ?? new NullNamingConvention();
			
			typeDescriptor.TypeDescriptor = 
				new YamlAttributesTypeDescriptor(
					new NamingConventionTypeDescriptor(
						new ReadableAndWritablePropertiesTypeDescriptor(),
						namingConvention
					)
				);

			converters = new List<IYamlTypeConverter>();
			Deserializers = new List<INodeDeserializer>();
			Deserializers.Add(new TypeConverterNodeDeserializer(converters));
			Deserializers.Add(new NullNodeDeserializer());
			Deserializers.Add(new ScalarNodeDeserializer());
			Deserializers.Add(new ArrayNodeDeserializer());
			Deserializers.Add(new GenericDictionaryNodeDeserializer(objectFactory));
			Deserializers.Add(new NonGenericDictionaryNodeDeserializer(objectFactory));
			Deserializers.Add(new GenericCollectionNodeDeserializer(objectFactory));
			Deserializers.Add(new NonGenericListNodeDeserializer(objectFactory));
			Deserializers.Add(new EnumerableNodeDeserializer());
			Deserializers.Add(new ObjectNodeDeserializer(objectFactory, typeDescriptor));

			tagMappings = new Dictionary<string, Type>(predefinedTagMappings);
			TypeResolvers = new List<INodeTypeResolver>();
			TypeResolvers.Add(new TagNodeTypeResolver(tagMappings));
			TypeResolvers.Add(new TypeNameInTagNodeTypeResolver());
			TypeResolvers.Add(new DefaultContainersNodeTypeResolver());
			
			base.SetValueDeserializer(
				new AliasValueDeserializer(
					new NodeValueDeserializer(
						Deserializers,
						TypeResolvers
					)
				)
			);
		}

		public void RegisterTagMapping(string tag, Type type)
		{
			tagMappings.Add(tag, type);
		}

		public void RegisterTypeConverter(IYamlTypeConverter typeConverter)
		{
			converters.Add(typeConverter);
		}
	}
}