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
using System.Collections.Generic;
using System.Globalization;

namespace YamlDotNet.Serialization.ObjectGraphVisitors
{
    public sealed class AnchorAssigner : IObjectGraphVisitor<Nothing>, IAliasProvider
    {
        private class AnchorAssignment
        {
            public string Anchor;
        }

        private readonly IDictionary<object, AnchorAssignment> assignments = new Dictionary<object, AnchorAssignment>();
        private uint nextId;

        bool IObjectGraphVisitor<Nothing>.Enter(IObjectDescriptor value, Nothing context)
        {
            AnchorAssignment assignment;
            if (value.Value != null && assignments.TryGetValue(value.Value, out assignment))
            {
                if (assignment.Anchor == null)
                {
                    assignment.Anchor = "o" + nextId.ToString(CultureInfo.InvariantCulture);
                    ++nextId;
                }
                return false;
            }

            return true;
        }

        bool IObjectGraphVisitor<Nothing>.EnterMapping(IObjectDescriptor key, IObjectDescriptor value, Nothing context)
        {
            return true;
        }

        bool IObjectGraphVisitor<Nothing>.EnterMapping(IPropertyDescriptor key, IObjectDescriptor value, Nothing context)
        {
            return true;
        }

        void IObjectGraphVisitor<Nothing>.VisitScalar(IObjectDescriptor scalar, Nothing context)
        {
            // Do not assign anchors to scalars
        }

        void IObjectGraphVisitor<Nothing>.VisitMappingStart(IObjectDescriptor mapping, Type keyType, Type valueType, Nothing context)
        {
            VisitObject(mapping);
        }

        void IObjectGraphVisitor<Nothing>.VisitMappingEnd(IObjectDescriptor mapping, Nothing context) { }

        void IObjectGraphVisitor<Nothing>.VisitSequenceStart(IObjectDescriptor sequence, Type elementType, Nothing context)
        {
            VisitObject(sequence);
        }

        void IObjectGraphVisitor<Nothing>.VisitSequenceEnd(IObjectDescriptor sequence, Nothing context) { }

        private void VisitObject(IObjectDescriptor value)
        {
            if(value.Value != null)
            {
                assignments.Add(value.Value, new AnchorAssignment());
            }
        }

        string IAliasProvider.GetAlias(object target)
        {
            AnchorAssignment assignment;
            if (target != null && assignments.TryGetValue(target, out assignment))
            {
                return assignment.Anchor;
            }
            return null;
        }
    }
}