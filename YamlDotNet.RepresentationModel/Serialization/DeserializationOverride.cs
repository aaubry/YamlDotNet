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
using YamlDotNet.Core;

namespace YamlDotNet.RepresentationModel.Serialization
{
	/// <summary>
	/// 
	/// </summary>
	public class DeserializationOverride
	{
		private readonly Type deserializedType;

		/// <summary>
		/// Gets type that contains the property.
		/// </summary>
		public Type DeserializedType
		{
			get
			{
				return deserializedType;
			}
		}

		private readonly string deserializedPropertyName;

		/// <summary>
		/// Gets the name of the deserialized property.
		/// </summary>
		public string DeserializedPropertyName
		{
			get
			{
				return deserializedPropertyName;
			}
		}

		private readonly Action<object, EventReader> deserializer;

		/// <summary>
		/// Gets the delegate that will perform the deserialization.
		/// </summary>
		public Action<object, EventReader> Deserializer
		{
			get
			{
				return deserializer;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DeserializationOverride"/> class.
		/// </summary>
		/// <param name="deserializedType">The type that contains the property.</param>
		/// <param name="deserializedPropertyName">Name of the deserialized property.</param>
		/// <param name="deserializer">The delegate that will perform the deserialization.</param>
		public DeserializationOverride(Type deserializedType, string deserializedPropertyName, Action<object, EventReader> deserializer)
		{
			this.deserializedType = deserializedType;
			this.deserializedPropertyName = deserializedPropertyName;
			this.deserializer = deserializer;
		}
	}
}