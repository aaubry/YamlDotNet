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

namespace YamlDotNet.Serialization.ObjectFactories
{
    /// <summary>
    /// Gets information about and creates statically known, serializable, types.
    /// </summary>
    public abstract class StaticObjectFactory : IObjectFactory
    {
        /// <summary>
        /// Create an object of the specified type
        /// </summary>
        /// <param name="type">Type of object to create</param>
        /// <returns></returns>
        public abstract object Create(Type type);

        /// <summary>
        /// Creates an array of the specified type with the size specified
        /// </summary>
        /// <param name="type">The type of the array, should be the whole type, not just the value type</param>
        /// <param name="count">How large the array should be</param>
        /// <returns></returns>
        public abstract Array CreateArray(Type type, int count);

        /// <summary>
        /// Gets whether the type is a dictionary or not
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <returns></returns>
        public abstract bool IsDictionary(Type type);

        /// <summary>
        /// Gets whether the type is an array or not
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <returns></returns>
        public abstract bool IsArray(Type type);

        /// <summary>
        /// Gets whether the type is a list
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <returns></returns>
        public abstract bool IsList(Type type);

        /// <summary>
        /// Gets the type of the key of a dictionary
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public abstract Type GetKeyType(Type type);

        /// <summary>
        /// Gets the type of the value part of a dictionary or list.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public abstract Type GetValueType(Type type);

        /// <summary>
        /// Creates the default value of primitive types
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual object? CreatePrimitive(Type type)
        {
            var typeCode = Type.GetTypeCode(type);
            switch (typeCode)
            {
                case TypeCode.Boolean: return false;
                case TypeCode.Byte: return (byte)0;
                case TypeCode.Int16: return (short)0;
                case TypeCode.Int32: return (int)0;
                case TypeCode.Int64: return (long)0;
                case TypeCode.SByte: return (sbyte)0;
                case TypeCode.UInt16: return (ushort)0;
                case TypeCode.UInt32: return (uint)0;
                case TypeCode.UInt64: return (ulong)0;
                case TypeCode.Single: return (float)0;
                case TypeCode.Double: return (double)0;
                case TypeCode.Decimal: return (decimal)0;
                case TypeCode.Char: return (char)0;
                case TypeCode.DateTime: return new DateTime();
                default:
                    return null;
            }
        }

        /// <summary>
        /// The static implementation of yamldotnet doesn't support generating types, so we will return null's and false since we can't do anything.
        /// </summary>
        /// <param name="descriptor"></param>
        /// <param name="dictionary"></param>
        /// <param name="genericArguments"></param>
        /// <returns></returns>
        public bool GetDictionary(IObjectDescriptor descriptor, out IDictionary? dictionary, out Type[]? genericArguments)
        {
            dictionary = null;
            genericArguments = null;
            return false;
        }

        public abstract void ExecuteOnDeserializing(object value);

        public abstract void ExecuteOnDeserialized(object value);

        public abstract void ExecuteOnSerializing(object? value);

        public abstract void ExecuteOnSerialized(object? value);
    }
}
