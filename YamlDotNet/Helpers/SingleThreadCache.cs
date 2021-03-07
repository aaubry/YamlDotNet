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
using System.Diagnostics.CodeAnalysis;

namespace YamlDotNet.Helpers
{
    internal sealed class SingleThreadCache<TKey, TValue> : ICache<TKey, TValue>
        where TKey : notnull
    {
        private sealed class CacheEntry
        {
            private enum ValueState
            {
                NotComputed,
                Computing,
                Available,
            }

            private TValue value;
            private TwoStepFactory<TKey, TValue>? valueFactory;
            private ValueState valueState;

            public CacheEntry(TValue value)
            {
                this.value = value;
                this.valueState = ValueState.Available;
            }

            public CacheEntry(TwoStepFactory<TKey, TValue> valueFactory)
            {
                this.value = default!;
                this.valueFactory = valueFactory ?? throw new ArgumentNullException(nameof(valueFactory));
                this.valueState = ValueState.NotComputed;
            }

            public TValue GetValue(TKey key)
            {
                switch (valueState)
                {
                    case ValueState.NotComputed:
                        try
                        {
                            ComputeValue(key);
                        }
                        catch (InvalidRecursionException ex)
                        {
                            ex.AddPath(key.ToString()!);
                            throw;
                        }
                        break;

                    case ValueState.Computing:
                        throw new InvalidRecursionException(
                            "The valueFactory that was passed to SingleThreadCache.GetOrAdd() attempted to call itself.",
                            key.ToString()!
                        );
                }

                return this.value;
            }

            private void ComputeValue(TKey key)
            {
                this.valueState = ValueState.Computing;
                var (value, completeCreation) = valueFactory!(key);
                this.value = value;
                this.valueState = ValueState.Available;

                completeCreation?.Invoke();

                this.valueFactory = null;
            }
        }

        private readonly Dictionary<TKey, CacheEntry> entries = new Dictionary<TKey, CacheEntry>();

        public void Add(TKey key, TValue value) => entries.Add(key, new CacheEntry(value));

        public TValue GetOrAdd(TKey key, TValue value)
        {
            if (!entries.TryGetValue(key, out var entry))
            {
                entry = new CacheEntry(value);
                entries.Add(key, entry);
            }
            return entry.GetValue(key);
        }

        public TValue GetOrAdd(TKey key, TwoStepFactory<TKey, TValue> valueFactory)
        {
            if (!entries.TryGetValue(key, out var entry))
            {
                entry = new CacheEntry(valueFactory);
                entries.Add(key, entry);
            }
            return entry.GetValue(key);
        }

        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            if (entries.TryGetValue(key, out var lazyValue))
            {
                value = lazyValue.GetValue(key);
                return true;
            }

            value = default!;
            return false;
        }
    }
}
