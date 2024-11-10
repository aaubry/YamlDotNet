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
using System.Collections.Generic;
using YamlDotNet.Helpers;
using YamlDotNet.Serialization.Utilities;

namespace YamlDotNet.Serialization.ObjectFactories
{
    public abstract class ObjectFactoryBase : IObjectFactory
    {
        public abstract object Create(Type type);

        public virtual object? CreatePrimitive(Type type) => type.IsValueType() ? Activator.CreateInstance(type) : null;

        /// <inheritdoc />
        public virtual void ExecuteOnDeserialized(object value)
        {
        }

        /// <inheritdoc />
        public virtual void ExecuteOnDeserializing(object value)
        {
        }

        /// <inheritdoc />
        public virtual void ExecuteOnSerialized(object? value)
        {
        }

        /// <inheritdoc />
        public virtual void ExecuteOnSerializing(object? value)
        {
        }

        public virtual bool GetDictionary(IObjectDescriptor descriptor, out IDictionary? dictionary, out Type[]? genericArguments)
        {
            var genericDictionaryType = descriptor.Type.GetImplementationOfOpenGenericInterface(typeof(IDictionary<,>));
            if (genericDictionaryType != null)
            {
                genericArguments = genericDictionaryType.GetGenericArguments();
                var adaptedDictionary = Activator.CreateInstance(typeof(GenericDictionaryToNonGenericAdapter<,>).MakeGenericType(genericArguments), descriptor.Value)!;
                dictionary = adaptedDictionary as IDictionary;
                return true;
            }
            genericArguments = null;
            dictionary = null;
            return false;
        }

        public virtual Type GetValueType(Type type)
        {
            var enumerableType = type.GetImplementationOfOpenGenericInterface(typeof(IEnumerable<>));
            var itemType = enumerableType != null ? enumerableType.GetGenericArguments()[0] : typeof(object);
            return itemType;
        }
    }
}
