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

﻿using System;
using System.Linq;
using System.Collections.Generic;
using YamlDotNet.Core;

namespace YamlDotNet.RepresentationModel.Serialization
{
	/// <summary>
	/// Allows to register delegates that take care of the deserialization process of specific properties.
	/// </summary>
	public sealed class DeserializationOverrides
	{
		private readonly IDictionary<Type, Dictionary<string, Action<object, EventReader>>> overrides;

		/// <summary>
		/// Initializes a new instance of the <see cref="DeserializationOverrides"/> class.
		/// </summary>
		public DeserializationOverrides()
		{
			overrides = new Dictionary<Type, Dictionary<string, Action<object, EventReader>>>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DeserializationOverrides"/> class.
		/// </summary>
		/// <param name="overrides">The overrides.</param>
		public DeserializationOverrides(IEnumerable<DeserializationOverride> overrides)
		{
			var overridesByType = from over in overrides
								  group over by over.DeserializedType into byType
								  select new
								  {
									  Type = byType.Key,
									  Overrides = byType.ToDictionary(o => o.DeserializedPropertyName, o => o.Deserializer),
								  };

			this.overrides = overridesByType.ToDictionary(o => o.Type, o => o.Overrides);
		}

		/// <summary>
		/// Adds an override for the specified property.
		/// </summary>
		/// <typeparam name="TDeserialized">The type that contains the property.</typeparam>
		/// <param name="deserializedPropertyName">Name of the deserialized property.</param>
		/// <param name="deserializer">The delegate that will perform the deserialization.</param>
		public void Add<TDeserialized>(string deserializedPropertyName, Action<TDeserialized, EventReader> deserializer)
		{
			Add(typeof(TDeserialized), deserializedPropertyName, (target, reader) => deserializer((TDeserialized)target, reader));
		}

		/// <summary>
		/// Adds an override for the specified property.
		/// </summary>
		/// <param name="over">The override.</param>
		public void Add(DeserializationOverride over)
		{
			Add(over.DeserializedType, over.DeserializedPropertyName, over.Deserializer);
		}

		/// <summary>
		/// Adds an override for the specified property.
		/// </summary>
		/// <param name="deserializedType">The type that contains the property.</param>
		/// <param name="deserializedPropertyName">Name of the deserialized property.</param>
		/// <param name="deserializer">The delegate that will perform the deserialization.</param>
		public void Add(Type deserializedType, string deserializedPropertyName, Action<object, EventReader> deserializer)
		{
			if (deserializedType == null)
			{
				throw new ArgumentNullException("deserializedType");
			}
			if (string.IsNullOrEmpty(deserializedPropertyName))
			{
				throw new ArgumentNullException("deserializedPropertyName");
			}
			if (deserializer == null)
			{
				throw new ArgumentNullException("deserializer");
			}

			Dictionary<string, Action<object, EventReader>> typeOverrides;
			if (!overrides.TryGetValue(deserializedType, out typeOverrides))
			{
				typeOverrides = new Dictionary<string, Action<object, EventReader>>();
				overrides.Add(deserializedType, typeOverrides);
			}
			typeOverrides.Add(deserializedPropertyName, deserializer);
		}

		internal Action<object, EventReader> GetOverride(Type deserializedType, string deserializedPropertyName)
		{
			Dictionary<string, Action<object, EventReader>> typeOverrides;
			if (overrides.TryGetValue(deserializedType, out typeOverrides))
			{
				Action<object, EventReader> deserializer;
				if(typeOverrides.TryGetValue(deserializedPropertyName, out deserializer))
				{
					return deserializer;
				}
			}
			return null;
		}
	}
}