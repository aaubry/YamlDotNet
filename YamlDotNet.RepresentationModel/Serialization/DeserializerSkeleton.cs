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

namespace YamlDotNet.RepresentationModel.Serialization
{
	/// <summary>
	/// Reads objects from YAML.
	/// </summary>
	public class DeserializerSkeleton
	{
		public IList<INodeDeserializer> Deserializers { get; private set; }
		public IList<INodeTypeResolver> TypeResolvers { get; private set; }

		public DeserializerSkeleton()
		{
			Deserializers = new List<INodeDeserializer>();
			TypeResolvers = new List<INodeTypeResolver>();
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

			var result = DeserializeValue(reader, type);

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

		private object DeserializeValue(EventReader reader, Type expectedType)
		{
			var nodeEvent = reader.Peek<NodeEvent>();

			var nodeType = GetTypeFromEvent(nodeEvent, expectedType);

			foreach (var deserializer in Deserializers)
			{
				object value;
				if (deserializer.Deserialize(reader, nodeType, DeserializeValue, out value))
				{
					return value;
				}
			}

			throw new Exception("TODO");

			//return deserializer.Deserialize(reader, expectedType);

			//if (reader.Accept<AnchorAlias>())
			//{
			//	throw new NotImplementedException();
			//	//return context.Anchors[reader.Expect<AnchorAlias>().Value];
			//}

			//var nodeEvent = (NodeEvent)reader.Parser.Current;

			//if (IsNull(nodeEvent))
			//{
			//	reader.Expect<NodeEvent>();
			//	AddAnchoredObject(nodeEvent, null, context.Anchors);
			//	return null;
			//}

			//object result = DeserializeValueNotNull(reader, context, nodeEvent, expectedType);
			//return ObjectConverter.Convert(result, expectedType);
		}

		private Type GetTypeFromEvent(NodeEvent nodeEvent, Type currentType)
		{
			foreach (var typeResolver in TypeResolvers)
			{
				if (typeResolver.Resolve(nodeEvent, ref currentType))
				{
					break;
				}
			}
			return currentType;
		}
	}
}
