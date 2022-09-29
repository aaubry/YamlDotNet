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

namespace YamlDotNet.Serialization.ObjectGraphVisitors
{
    /// <summary>
    /// A base class that simplifies the correct implementation of <see cref="IObjectGraphVisitor{Nothing}" />.
    /// </summary>
    public abstract class PreProcessingPhaseObjectGraphVisitorSkeleton : IObjectGraphVisitor<Nothing>
    {
        protected readonly IEnumerable<IYamlTypeConverter> typeConverters;

        public PreProcessingPhaseObjectGraphVisitorSkeleton(IEnumerable<IYamlTypeConverter> typeConverters)
        {
            this.typeConverters = typeConverters != null
                ? typeConverters.ToList()
                : Enumerable.Empty<IYamlTypeConverter>();
        }

        bool IObjectGraphVisitor<Nothing>.Enter(IObjectDescriptor value, Nothing context)
        {
            var typeConverter = typeConverters.FirstOrDefault(t => t.Accepts(value.Type));
            if (typeConverter != null)
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

            return Enter(value);
        }

        bool IObjectGraphVisitor<Nothing>.EnterMapping(IPropertyDescriptor key, IObjectDescriptor value, Nothing context)
        {
            return EnterMapping(key, value);
        }

        bool IObjectGraphVisitor<Nothing>.EnterMapping(IObjectDescriptor key, IObjectDescriptor value, Nothing context)
        {
            return EnterMapping(key, value);
        }

        void IObjectGraphVisitor<Nothing>.VisitMappingEnd(IObjectDescriptor mapping, Nothing context)
        {
            VisitMappingEnd(mapping);
        }

        void IObjectGraphVisitor<Nothing>.VisitMappingStart(IObjectDescriptor mapping, Type keyType, Type valueType, Nothing context)
        {
            VisitMappingStart(mapping, keyType, valueType);
        }

        void IObjectGraphVisitor<Nothing>.VisitScalar(IObjectDescriptor scalar, Nothing context)
        {
            VisitScalar(scalar);
        }

        void IObjectGraphVisitor<Nothing>.VisitSequenceEnd(IObjectDescriptor sequence, Nothing context)
        {
            VisitSequenceEnd(sequence);
        }

        void IObjectGraphVisitor<Nothing>.VisitSequenceStart(IObjectDescriptor sequence, Type elementType, Nothing context)
        {
            VisitSequenceStart(sequence, elementType);
        }

        protected abstract bool Enter(IObjectDescriptor value);
        protected abstract bool EnterMapping(IPropertyDescriptor key, IObjectDescriptor value);
        protected abstract bool EnterMapping(IObjectDescriptor key, IObjectDescriptor value);
        protected abstract void VisitMappingEnd(IObjectDescriptor mapping);
        protected abstract void VisitMappingStart(IObjectDescriptor mapping, Type keyType, Type valueType);
        protected abstract void VisitScalar(IObjectDescriptor scalar);
        protected abstract void VisitSequenceEnd(IObjectDescriptor sequence);
        protected abstract void VisitSequenceStart(IObjectDescriptor sequence, Type elementType);
    }
}
