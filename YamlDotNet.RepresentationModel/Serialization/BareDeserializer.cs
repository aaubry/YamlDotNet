// This file is part of YamlDotNet - A .NET library for YAML.
// Copyright (c) 2013 aaubry
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using System.Runtime.Serialization;

namespace YamlDotNet.RepresentationModel.Serialization
{
	/// <summary>
	/// Reads objects from YAML. This class allows to fully customize the
	/// deserialization process. In most cases you should use <see cref="Deserializer"/> instead.
	/// </summary>
	public class BareDeserializer
	{
		private IValueDeserializer valueDeserializer;
	
		public BareDeserializer (IValueDeserializer valueDeserializer)
		{
			SetValueDeserializer(valueDeserializer);
		}
		
		internal BareDeserializer()
		{
		}
		
		internal void SetValueDeserializer(IValueDeserializer valueDeserializer)
		{
			if (valueDeserializer == null)
			{
				throw new ArgumentNullException ("valueDeserializer");
			}
			
			this.valueDeserializer = valueDeserializer;
		}

		public T Deserialize<T>(TextReader input, DeserializationFlags options = DeserializationFlags.None)
		{
			return (T)Deserialize(input, typeof(T), options);
		}

		public object Deserialize(TextReader input, DeserializationFlags options = DeserializationFlags.None)
		{
			return Deserialize(input, typeof(object), options);
		}

		public object Deserialize(TextReader input, Type type, DeserializationFlags options = DeserializationFlags.None)
		{
			return Deserialize(new EventReader(new Parser(input)), type, options);
		}

		public T Deserialize<T>(EventReader reader, DeserializationFlags options = DeserializationFlags.None)
		{
			return (T)Deserialize(reader, typeof(T), options);
		}

		public object Deserialize(EventReader reader, DeserializationFlags options = DeserializationFlags.None)
		{
			return Deserialize(reader, typeof(object), options);
		}

		/// <summary>
		/// Deserializes an object of the specified type.
		/// </summary>
		/// <param name="reader">The <see cref="EventReader" /> where to deserialize the object.</param>
		/// <param name="type">The static type of the object to deserialize.</param>
		/// <param name="options">Options that control how the deserialization is to be performed.</param>
		/// <returns>Returns the deserialized object.</returns>
		public object Deserialize(EventReader reader, Type type, DeserializationFlags options = DeserializationFlags.None)
		{
			if (reader == null)
			{
				throw new ArgumentNullException("reader");
			}

			if (type == null)
			{
				throw new ArgumentNullException("type");
			}

			var hasStreamStart = reader.Allow<StreamStart>() != null;

			var hasDocumentStart = reader.Allow<DocumentStart>() != null;

			var result = valueDeserializer.DeserializeValue(reader, type, new SerializerState(), valueDeserializer);

			if (hasDocumentStart)
			{
				reader.Expect<DocumentEnd>();
			}

			if (hasStreamStart)
			{
				reader.Expect<StreamEnd>();
			}

			return result;
		}
	}
}
