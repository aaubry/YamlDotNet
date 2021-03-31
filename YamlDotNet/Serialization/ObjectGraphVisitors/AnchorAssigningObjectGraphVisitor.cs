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
using YamlDotNet.Core;

namespace YamlDotNet.Serialization.ObjectGraphVisitors
{
    public sealed class AnchorAssigningObjectGraphVisitor : ChainedObjectGraphVisitor
    {
        private readonly IEventEmitter eventEmitter;
        private readonly IAliasProvider aliasProvider;
        private readonly HashSet<AnchorName> emittedAliases = new HashSet<AnchorName>();

        public AnchorAssigningObjectGraphVisitor(IObjectGraphVisitor<IEmitter> nextVisitor, IEventEmitter eventEmitter, IAliasProvider aliasProvider)
            : base(nextVisitor)
        {
            this.eventEmitter = eventEmitter;
            this.aliasProvider = aliasProvider;
        }

        public override bool Enter(IObjectDescriptor value, IEmitter context)
        {
            if (value.Value != null)
            {
                var alias = aliasProvider.GetAlias(value.Value);
                if (!alias.IsEmpty && !emittedAliases.Add(alias))
                {
                    var aliasEventInfo = new AliasEventInfo(value, alias);
                    eventEmitter.Emit(aliasEventInfo, context);
                    return aliasEventInfo.NeedsExpansion;
                }
            }
            return base.Enter(value, context);
        }

        public override void VisitMappingStart(IObjectDescriptor mapping, Type keyType, Type valueType, IEmitter context)
        {
            var anchor = aliasProvider.GetAlias(mapping.NonNullValue());
            eventEmitter.Emit(new MappingStartEventInfo(mapping) { Anchor = anchor }, context);
        }

        public override void VisitSequenceStart(IObjectDescriptor sequence, Type elementType, IEmitter context)
        {
            var anchor = aliasProvider.GetAlias(sequence.NonNullValue());
            eventEmitter.Emit(new SequenceStartEventInfo(sequence) { Anchor = anchor }, context);
        }

        public override void VisitScalar(IObjectDescriptor scalar, IEmitter context)
        {
            var scalarInfo = new ScalarEventInfo(scalar);
            if (scalar.Value != null)
            {
                scalarInfo.Anchor = aliasProvider.GetAlias(scalar.Value);
            }
            eventEmitter.Emit(scalarInfo, context);
        }
    }
}
