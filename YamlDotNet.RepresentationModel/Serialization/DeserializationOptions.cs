//  This file is part of YamlDotNet - A .NET library for YAML.
//  Copyright (c) 2008, 2009, 2010, 2011, 2012 Antoine Aubry
    
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

		private IObjectFactory objectFactory = new DefaultObjectFactory();

		/// <summary>
		/// Gets / sets the <see cref="IObjectFactory"/> that is used to create instances of objects when deserializing.
		/// </summary>
		public IObjectFactory ObjectFactory
		{
			get
			{
				return objectFactory;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("ObjectFactory");
				}
				objectFactory = value;
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