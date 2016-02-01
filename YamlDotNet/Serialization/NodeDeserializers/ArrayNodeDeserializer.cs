//  This file is part of YamlDotNet - A .NET library for YAML.
//  Copyright (c) Antoine Aubry and contributors

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
using System.Reflection;
using YamlDotNet.Core;
using YamlDotNet.Serialization.Utilities;

namespace YamlDotNet.Serialization.NodeDeserializers
{
	public sealed class ArrayNodeDeserializer : INodeDeserializer
	{
		bool INodeDeserializer.Deserialize(EventReader reader, Type expectedType, Func<EventReader, Type, object> nestedObjectDeserializer, out object value)
		{
			if (!expectedType.IsArray)
			{
				value = false;
				return false;
			}

			value = DeserializeHelper(expectedType.GetElementType(), reader, expectedType, nestedObjectDeserializer);
			return true;
		}

		private static object DeserializeHelper(Type tItem, EventReader reader, Type expectedType, Func<EventReader, Type, object> nestedObjectDeserializer)
		{
			// Create typed list
			IList items = (IList) typeof(List<object>).MakeGenericType( new Type[] { tItem } ).GetConstructor( new Type[] {} ).Invoke( new object[] {} );

			GenericCollectionNodeDeserializer.DeserializeHelper(tItem, reader, expectedType, nestedObjectDeserializer, items);

			// Create typed array
			ConstructorInfo arrayTypeConstructor = tItem.MakeArrayType().GetConstructor( new Type[] { typeof(int) } );
			object[] constructorArgs = new object[] { items.Count };
			object array = arrayTypeConstructor.Invoke(constructorArgs);

			// Assign from the list to the array
			var asIList = (IList) array;
			for (int i = 0; i < items.Count; i++) {
				asIList[i] = items[i];
			}

			return array;
		}
	}
}

