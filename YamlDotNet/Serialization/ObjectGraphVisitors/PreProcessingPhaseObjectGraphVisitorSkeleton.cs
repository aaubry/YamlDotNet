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
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization.Utilities;

namespace YamlDotNet.Serialization.ObjectGraphVisitors
{
    /// <summary>
    /// A base class that simplifies the correct implementation of <see cref="IObjectGraphVisitor{Nothing}" />.
    /// </summary>
    public abstract class PreProcessingPhaseObjectGraphVisitorSkeleton : IObjectGraphVisitor<Nothing>
    {
        protected readonly IEnumerable<IYamlTypeConverter> typeConverters; // This is kept for backwards compatibility for subclasses. Use typeConverterCache instead.
        private readonly TypeConverterCache typeConverterCache;

        public PreProcessingPhaseObjectGraphVisitorSkeleton(IEnumerable<IYamlTypeConverter> typeConverters)
        {
            var tcs = typeConverters?.ToArray() ?? new IYamlTypeConverter[]{};

            this.typeConverters = tcs;
            this.typeConverterCache = new TypeConverterCache(tcs);
        }

        bool IObjectGraphVisitor<Nothing>.Enter(IPropertyDescriptor? propertyDescriptor, IObjectDescriptor value, Nothing context, ObjectSerializer serializer)
        {
            if (typeConverterCache.TryGetConverterForType(value.Type, out _))
            {
                return false;
            }

            if (value.Value is IYamlConvertible convertible)
            {
                return false;
            }

#pragma warning disable 0618 // IYamlSerializable is obsolete
            if (value.Value is IYamlSerializable serializable)
            {
                return false;
            }
#pragma warning restore

            return Enter(value, serializer);
        }

        bool IObjectGraphVisitor<Nothing>.EnterMapping(IPropertyDescriptor key, IObjectDescriptor value, Nothing context, ObjectSerializer serializer)
        {
            return EnterMapping(key, value, serializer);
        }

        bool IObjectGraphVisitor<Nothing>.EnterMapping(IObjectDescriptor key, IObjectDescriptor value, Nothing context, ObjectSerializer serializer)
        {
            return EnterMapping(key, value, serializer);
        }

        void IObjectGraphVisitor<Nothing>.VisitMappingEnd(IObjectDescriptor mapping, Nothing context, ObjectSerializer serializer)
        {
            VisitMappingEnd(mapping, serializer);
        }

        void IObjectGraphVisitor<Nothing>.VisitMappingStart(IObjectDescriptor mapping, Type keyType, Type valueType, Nothing context, ObjectSerializer serializer)
        {
            VisitMappingStart(mapping, keyType, valueType, serializer);
        }

        void IObjectGraphVisitor<Nothing>.VisitScalar(IObjectDescriptor scalar, Nothing context, ObjectSerializer serializer)
        {
            VisitScalar(scalar, serializer);
        }

        void IObjectGraphVisitor<Nothing>.VisitSequenceEnd(IObjectDescriptor sequence, Nothing context, ObjectSerializer serializer)
        {
            VisitSequenceEnd(sequence, serializer);
        }

        void IObjectGraphVisitor<Nothing>.VisitSequenceStart(IObjectDescriptor sequence, Type elementType, Nothing context, ObjectSerializer serializer)
        {
            VisitSequenceStart(sequence, elementType, serializer);
        }

        protected abstract bool Enter(IObjectDescriptor value, ObjectSerializer serializer);
        protected abstract bool EnterMapping(IPropertyDescriptor key, IObjectDescriptor value, ObjectSerializer serializer);
        protected abstract bool EnterMapping(IObjectDescriptor key, IObjectDescriptor value, ObjectSerializer serializer);
        protected abstract void VisitMappingEnd(IObjectDescriptor mapping, ObjectSerializer serializer);
        protected abstract void VisitMappingStart(IObjectDescriptor mapping, Type keyType, Type valueType, ObjectSerializer serializer);
        protected abstract void VisitScalar(IObjectDescriptor scalar, ObjectSerializer serializer);
        protected abstract void VisitSequenceEnd(IObjectDescriptor sequence, ObjectSerializer serializer);
        protected abstract void VisitSequenceStart(IObjectDescriptor sequence, Type elementType, ObjectSerializer serializer);
    }
}
