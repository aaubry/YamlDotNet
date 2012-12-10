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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace YamlDotNet.RepresentationModel.Serialization
{
	/// <summary>
	/// Options that control the deserialization process.
	/// </summary>
	[Flags]
	public enum DeserializationFlags
	{
		/// <summary>
		/// Serializes using the default options
		/// </summary>
		None = 0,

		///// <summary>
		///// Ensures that it will be possible to deserialize the serialized objects.
		///// </summary>
		//Roundtrip = 1,

		///// <summary>
		///// If this flag is specified, if the same object appears more than once in the
		///// serialization graph, it will be serialized each time instead of just once.
		///// </summary>
		///// <remarks>
		///// If the serialization graph contains circular references and this flag is set,
		///// a <see cref="StackOverflowException" /> will be thrown.
		///// If this flag is not set, there is a performance penalty because the entire
		///// object graph must be walked twice.
		///// </remarks>
		//DisableAliases = 2,

		///// <summary>
		///// Forces every value to be serialized, even if it is the default value for that type.
		///// </summary>
		//EmitDefaults = 4,

		/// <summary>
		/// Ensures that the result of the serialization is valid JSON.
		/// </summary>
		JsonCompatible = 8,
	}

	/// <summary>
	/// Reads objects from YAML.
	/// </summary>
	public class Deserializer
	{
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

			object result = DeserializeValue(reader, type, null);

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

		private object DeserializeValue(EventReader reader, Type expectedType, object context)
		{
			if (reader.Accept<AnchorAlias>())
			{
				throw new NotImplementedException();
				//return context.Anchors[reader.Expect<AnchorAlias>().Value];
			}

			var nodeEvent = (NodeEvent)reader.Parser.Current;

			if (IsNull(nodeEvent))
			{
				reader.Expect<NodeEvent>();
				AddAnchoredObject(nodeEvent, null, context.Anchors);
				return null;
			}

			object result = DeserializeValueNotNull(reader, context, nodeEvent, expectedType);
			return ObjectConverter.Convert(result, expectedType);
		}

		//private bool IsNull(NodeEvent nodeEvent)
		//{
		//	if (nodeEvent.Tag == "tag:yaml.org,2002:null")
		//	{
		//		return true;
		//	}

		//	if (JsonCompatible)
		//	{
		//		var scalar = nodeEvent as Scalar;
		//		if (scalar != null && scalar.Style == Core.ScalarStyle.Plain && scalar.Value == "null")
		//		{
		//			return true;
		//		}
		//	}

		//	return false;
		//}
	}
}