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
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization.Utilities;

namespace YamlDotNet.Serialization.ValueDeserializers
{
    public sealed class AliasValueDeserializer : IValueDeserializer
    {
        private readonly IValueDeserializer innerDeserializer;

        public AliasValueDeserializer(IValueDeserializer innerDeserializer)
        {
            this.innerDeserializer = innerDeserializer ?? throw new ArgumentNullException(nameof(innerDeserializer));
        }

        private sealed class AliasState : Dictionary<AnchorName, ValuePromise>, IPostDeserializationCallback
        {
            public void OnDeserialization()
            {
                foreach (var promise in Values)
                {
                    if (!promise.HasValue)
                    {
                        var alias = promise.Alias!; // When the value is not known, the alias is always known
                        throw new AnchorNotFoundException(alias.Start, alias.End, $"Anchor '{alias.Value}' not found");
                    }
                }
            }
        }

        private sealed class ValuePromise : IValuePromise
        {
            public event Action<object?>? ValueAvailable;

            public bool HasValue { get; private set; }

            private object? value;

            public readonly AnchorAlias? Alias;

            public ValuePromise(AnchorAlias alias)
            {
                this.Alias = alias;
            }

            public ValuePromise(object? value)
            {
                HasValue = true;
                this.value = value;
            }

            public object? Value
            {
                get
                {
                    if (!HasValue)
                    {
                        throw new InvalidOperationException("Value not set");
                    }
                    return value;
                }
                set
                {
                    if (HasValue)
                    {
                        throw new InvalidOperationException("Value already set");
                    }
                    HasValue = true;
                    this.value = value;

                    ValueAvailable?.Invoke(value);
                }
            }
        }

        public object? DeserializeValue(IParser parser, Type expectedType, SerializerState state, IValueDeserializer nestedObjectDeserializer)
        {
            object? value;
            if (parser.TryConsume<AnchorAlias>(out var alias))
            {
                var aliasState = state.Get<AliasState>();
                if (!aliasState.TryGetValue(alias.Value, out var valuePromise))
                {
                    throw new AnchorNotFoundException(alias.Start, alias.End, $"Alias ${alias.Value} cannot precede anchor declaration");
                }

                return valuePromise.HasValue ? valuePromise.Value : valuePromise;
            }

            var anchor = AnchorName.Empty;
            if (parser.Accept<NodeEvent>(out var nodeEvent) && !nodeEvent.Anchor.IsEmpty)
            {
                anchor = nodeEvent.Anchor;
                var aliasState = state.Get<AliasState>();
                if (!aliasState.ContainsKey(anchor))
                {
                    aliasState[anchor] = new ValuePromise(new AnchorAlias(anchor));
                }
            }

            value = innerDeserializer.DeserializeValue(parser, expectedType, state, nestedObjectDeserializer);

            if (!anchor.IsEmpty)
            {
                var aliasState = state.Get<AliasState>();

                if (!aliasState.TryGetValue(anchor, out var valuePromise))
                {
                    aliasState.Add(anchor, new ValuePromise(value));
                }
                else if (!valuePromise.HasValue)
                {
                    valuePromise.Value = value;
                }
                else
                {
                    aliasState[anchor] = new ValuePromise(value);
                }
            }

            return value;
        }
    }
}
