// This file is part of YamlDotNet - A .NET library for YAML.
// Copyright (c) Antoine Aubry and contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
// of the Software, and to permit persons to whom the Software is furnished to do
// so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections;

namespace YamlDotNet.Serialization
{
    /// <summary>
    /// Creates instances of types.
    /// </summary>
    /// <remarks>
    /// This interface allows to provide a custom logic for creating instances during deserialization.
    /// </remarks>
    public interface IObjectFactory
    {
        /// <summary>
        /// Creates an instance of the specified type.
        /// </summary>
        object Create(Type type);

        /// <summary>
        /// Creates a default value for the .net primitive types (string, int, bool, etc)
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        object? CreatePrimitive(Type type);

        /// <summary>
        /// If the type is convertable to a non generic dictionary, then it will do so and set dictionary and genericArguments to the correct values and return true.
        /// If not, values will be null and the result will be false..
        /// </summary>
        /// <param name="descriptor">Object descriptor to try and convert</param>
        /// <param name="dictionary">The converted dictionary</param>
        /// <param name="genericArguments">Generic type arguments that specify the key and value type</param>
        /// <returns>True if converted, false if not</returns>
        bool GetDictionary(IObjectDescriptor descriptor, out IDictionary? dictionary, out Type[]? genericArguments);

        /// <summary>
        /// Gets the type of the value part of a dictionary or list.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        Type GetValueType(Type type);

        /// <summary>
        /// Executes the methods on the object that has the <seealso cref="Callbacks.OnDeserializingAttribute"/> attribute
        /// </summary>
        /// <param name="value"></param>
        void ExecuteOnDeserializing(object value);

        /// <summary>
        /// Executes the methods on the object that has the <seealso cref="Callbacks.OnDeserializedAttribute"/> attribute
        /// </summary>
        /// <param name="value"></param>
        void ExecuteOnDeserialized(object value);

        /// <summary>
        /// Executes the methods on the object that has the <seealso cref="Callbacks.OnSerializingAttribute"/> attribute
        /// </summary>
        /// <param name="value"></param>
        void ExecuteOnSerializing(object? value);

        /// <summary>
        /// Executes the methods on the object that has the <seealso cref="Callbacks.OnSerializedAttribute"/> attribute
        /// </summary>
        /// <param name="value"></param>
        void ExecuteOnSerialized(object? value);
    }
}
