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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace System.Collections.Concurrent
{
    internal sealed class ConcurrentDictionary<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> entries = new Dictionary<TKey, TValue>();

        public delegate TValue ValueFactory(TKey key);

        public TValue GetOrAdd(TKey key, ValueFactory valueFactory)
        {
            lock (entries)
            {
                if (!entries.TryGetValue(key, out var value))
                {
                    value = valueFactory(key);
                    entries.Add(key, value);
                }
                return value;
            }
        }

        public bool TryAdd(TKey key, TValue value)
        {
            lock (entries)
            {
                if (!entries.ContainsKey(key))
                {
                    entries.Add(key, value);
                    return true;
                }
                return false;
            }
        }

        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            lock (entries)
            {
                return entries.TryGetValue(key, out value);
            }
        }
    }
}
