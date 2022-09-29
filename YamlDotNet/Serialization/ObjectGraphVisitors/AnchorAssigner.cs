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
using System.Globalization;
using YamlDotNet.Core;

namespace YamlDotNet.Serialization.ObjectGraphVisitors
{
    public sealed class AnchorAssigner : PreProcessingPhaseObjectGraphVisitorSkeleton, IAliasProvider
    {
        private class AnchorAssignment
        {
            public AnchorName Anchor;
        }

        private readonly IDictionary<object, AnchorAssignment> assignments = new Dictionary<object, AnchorAssignment>();
        private uint nextId;

        public AnchorAssigner(IEnumerable<IYamlTypeConverter> typeConverters)
            : base(typeConverters)
        {
        }

        protected override bool Enter(IObjectDescriptor value)
        {
            if (value.Value != null && assignments.TryGetValue(value.Value, out var assignment))
            {
                if (assignment.Anchor.IsEmpty)
                {
                    assignment.Anchor = new AnchorName("o" + nextId.ToString(CultureInfo.InvariantCulture));
                    ++nextId;
                }
                return false;
            }

            return true;
        }

        protected override bool EnterMapping(IObjectDescriptor key, IObjectDescriptor value)
        {
            return true;
        }

        protected override bool EnterMapping(IPropertyDescriptor key, IObjectDescriptor value)
        {
            return true;
        }

        protected override void VisitScalar(IObjectDescriptor scalar)
        {
            // Do not assign anchors to scalars
        }

        protected override void VisitMappingStart(IObjectDescriptor mapping, Type keyType, Type valueType)
        {
            VisitObject(mapping);
        }

        protected override void VisitMappingEnd(IObjectDescriptor mapping) { }

        protected override void VisitSequenceStart(IObjectDescriptor sequence, Type elementType)
        {
            VisitObject(sequence);
        }

        protected override void VisitSequenceEnd(IObjectDescriptor sequence) { }

        private void VisitObject(IObjectDescriptor value)
        {
            if (value.Value != null)
            {
                assignments.Add(value.Value, new AnchorAssignment());
            }
        }

        AnchorName IAliasProvider.GetAlias(object target)
        {
            if (target != null && assignments.TryGetValue(target, out var assignment))
            {
                return assignment.Anchor;
            }
            return AnchorName.Empty;
        }
    }
}
