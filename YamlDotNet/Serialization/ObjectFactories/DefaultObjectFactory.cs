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
using System.Linq;
using System.Reflection;
using YamlDotNet.Serialization.Callbacks;

namespace YamlDotNet.Serialization.ObjectFactories
{
    /// <summary>
    /// Creates objects using Activator.CreateInstance.
    /// </summary>
    public sealed class DefaultObjectFactory : ObjectFactoryBase
    {
        private readonly Dictionary<Type, Dictionary<Type, MethodInfo[]>> _stateMethods = new Dictionary<Type, Dictionary<Type, MethodInfo[]>>
        {
            { typeof(OnDeserializedAttribute), new Dictionary<Type, MethodInfo[]>() },
            { typeof(OnDeserializingAttribute), new Dictionary<Type, MethodInfo[]>() },
            { typeof(OnSerializedAttribute), new Dictionary<Type, MethodInfo[]>() },
            { typeof(OnSerializingAttribute), new Dictionary<Type, MethodInfo[]>() },
        };

        private readonly Dictionary<Type, Type> DefaultGenericInterfaceImplementations = new Dictionary<Type, Type>
        {
            { typeof(IEnumerable<>), typeof(List<>) },
            { typeof(ICollection<>), typeof(List<>) },
            { typeof(IList<>), typeof(List<>) },
            { typeof(IDictionary<,>), typeof(Dictionary<,>) }
        };

        private readonly Dictionary<Type, Type> DefaultNonGenericInterfaceImplementations = new Dictionary<Type, Type>
        {
            { typeof(IEnumerable), typeof(List<object>) },
            { typeof(ICollection), typeof(List<object>) },
            { typeof(IList), typeof(List<object>) },
            { typeof(IDictionary), typeof(Dictionary<object, object>) }
        };

        private readonly Settings settings;

        public DefaultObjectFactory()
            : this(new Dictionary<Type, Type>(), new Settings())
        {
        }

        public DefaultObjectFactory(IDictionary<Type, Type> mappings)
            : this(mappings, new Settings())
        {
        }

        public DefaultObjectFactory(IDictionary<Type, Type> mappings, Settings settings)
        {
            foreach (var pair in mappings)
            {
                if (!pair.Key.IsAssignableFrom(pair.Value))
                {
                    throw new InvalidOperationException($"Type '{pair.Value}' does not implement type '{pair.Key}'.");
                }

                DefaultNonGenericInterfaceImplementations.Add(pair.Key, pair.Value);
            }

            this.settings = settings;
        }

        public override object Create(Type type)
        {
            if (type.IsInterface())
            {
                if (type.IsGenericType())
                {
                    if (DefaultGenericInterfaceImplementations.TryGetValue(type.GetGenericTypeDefinition(), out var implementationType))
                    {
                        type = implementationType.MakeGenericType(type.GetGenericArguments());
                    }
                }
                else
                {
                    if (DefaultNonGenericInterfaceImplementations.TryGetValue(type, out var implementationType))
                    {
                        type = implementationType;
                    }
                }
            }

            try
            {
                return Activator.CreateInstance(type, settings.AllowPrivateConstructors)!;
            }
            catch (Exception err)
            {
                var message = $"Failed to create an instance of type '{type.FullName}'.";
                throw new InvalidOperationException(message, err);
            }
        }

        public override void ExecuteOnDeserialized(object value) =>
            ExecuteState(typeof(OnDeserializedAttribute), value);

        public override void ExecuteOnDeserializing(object value) =>
            ExecuteState(typeof(OnDeserializingAttribute), value);

        public override void ExecuteOnSerialized(object value) =>
            ExecuteState(typeof(OnSerializedAttribute), value);

        public override void ExecuteOnSerializing(object value) =>
            ExecuteState(typeof(OnSerializingAttribute), value);

        private void ExecuteState(Type attributeType, object value)
        {
            if (value == null)
            {
                return;
            }

            var type = value.GetType();
            var methodsToExecute = GetStateMethods(attributeType, type);

            foreach (var method in methodsToExecute)
            {
                method.Invoke(value, null);
            }
        }

        private MethodInfo[] GetStateMethods(Type attributeType, Type valueType)
        {
            var stateDictionary = _stateMethods[attributeType];

            if (stateDictionary.TryGetValue(valueType, out var methods))
            {
                return methods;
            }

            methods = valueType.GetMethods(BindingFlags.Public |
                                          BindingFlags.Instance |
                                          BindingFlags.NonPublic);
            methods = methods.Where(x => x.GetCustomAttributes(attributeType, true).Length > 0).ToArray();
            stateDictionary[valueType] = methods;
            return methods;
        }
    }
}
