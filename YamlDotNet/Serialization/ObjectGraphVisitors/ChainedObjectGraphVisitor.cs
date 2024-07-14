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
using YamlDotNet.Core;

namespace YamlDotNet.Serialization.ObjectGraphVisitors
{
    public abstract class ChainedObjectGraphVisitor : IObjectGraphVisitor<IEmitter>
    {
        private readonly IObjectGraphVisitor<IEmitter> nextVisitor;

        protected ChainedObjectGraphVisitor(IObjectGraphVisitor<IEmitter> nextVisitor)
        {
            this.nextVisitor = nextVisitor;
        }

        public virtual bool Enter(IPropertyDescriptor? propertyDescriptor, IObjectDescriptor value, IEmitter context, ObjectSerializer serializer)
        {
            return nextVisitor.Enter(propertyDescriptor, value, context, serializer);
        }

        public virtual bool EnterMapping(IObjectDescriptor key, IObjectDescriptor value, IEmitter context, ObjectSerializer serializer)
        {
            return nextVisitor.EnterMapping(key, value, context, serializer);
        }

        public virtual bool EnterMapping(IPropertyDescriptor key, IObjectDescriptor value, IEmitter context, ObjectSerializer serializer)
        {
            return nextVisitor.EnterMapping(key, value, context, serializer);
        }

        public virtual void VisitScalar(IObjectDescriptor scalar, IEmitter context, ObjectSerializer serializer)
        {
            nextVisitor.VisitScalar(scalar, context, serializer);
        }

        public virtual void VisitMappingStart(IObjectDescriptor mapping, Type keyType, Type valueType, IEmitter context, ObjectSerializer serializer)
        {
            nextVisitor.VisitMappingStart(mapping, keyType, valueType, context, serializer);
        }

        public virtual void VisitMappingEnd(IObjectDescriptor mapping, IEmitter context, ObjectSerializer serializer)
        {
            nextVisitor.VisitMappingEnd(mapping, context, serializer);
        }

        public virtual void VisitSequenceStart(IObjectDescriptor sequence, Type elementType, IEmitter context, ObjectSerializer serializer)
        {
            nextVisitor.VisitSequenceStart(sequence, elementType, context, serializer);
        }

        public virtual void VisitSequenceEnd(IObjectDescriptor sequence, IEmitter context, ObjectSerializer serializer)
        {
            nextVisitor.VisitSequenceEnd(sequence, context, serializer);
        }
    }
}
