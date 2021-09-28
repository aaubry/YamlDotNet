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

using System.Collections;
using System.Collections.Generic;

namespace System.Linq.Expressions
{
    // Do not remove.
    // Avoids code breaking on .NET 2.0 due to using System.Linq.Expressions.
}

namespace System.Linq
{
    internal static partial class Enumerable
    {
        public static List<T> ToList<T>(this IEnumerable<T> sequence)
        {
            return new List<T>(sequence);
        }

        public static T[] ToArray<T>(this IEnumerable<T> sequence)
        {
            return sequence.ToList().ToArray();
        }

        public static IEnumerable<T> OrderBy<T, TKey>(this IEnumerable<T> sequence, Func<T, TKey> orderBy)
        {
            var comparer = Comparer<TKey>.Default;
            var list = sequence.ToList();
            list.Sort((a, b) => comparer.Compare(orderBy(a), orderBy(b)));
            return list;
        }

        public static IEnumerable<T> Empty<T>()
        {
            yield break;
        }

        public static IEnumerable<T> Where<T>(this IEnumerable<T> sequence, Func<T, bool> predicate)
        {
            foreach (var item in sequence)
            {
                if (predicate(item))
                {
                    yield return item;
                }
            }
        }

        public static IEnumerable<T2> Select<T1, T2>(this IEnumerable<T1> sequence, Func<T1, T2> selector)
        {
            foreach (var item in sequence)
            {
                yield return selector(item);
            }
        }

        public static IEnumerable<T> OfType<T>(this IEnumerable sequence)
        {
            foreach (var item in sequence)
            {
                if (item is T t)
                {
                    yield return t;
                }
            }
        }

        public static IEnumerable<T> Cast<T>(this IEnumerable sequence)
        {
            foreach (var item in sequence)
            {
                yield return (T)item;
            }
        }

        public static IEnumerable<T2> SelectMany<T1, T2>(this IEnumerable<T1> sequence, Func<T1, IEnumerable<T2>> selector)
        {
            foreach (var item in sequence)
            {
                foreach (var subitem in selector(item))
                {
                    yield return subitem;
                }
            }
        }

        public static T First<T>(this IEnumerable<T> sequence)
        {
            foreach (var item in sequence)
            {
                return item;
            }
            throw new InvalidOperationException();
        }

        public static T First<T>(this IEnumerable<T> sequence, Func<T, bool> predicate)
        {
            return sequence.Where(predicate).First();
        }

        public static T FirstOrDefault<T>(this IEnumerable<T> sequence)
        {
            foreach (var item in sequence)
            {
                return item;
            }
            return default!;
        }

        public static T FirstOrDefault<T>(this IEnumerable<T> sequence, Func<T, bool> predicate)
        {
            return sequence.Where(predicate).FirstOrDefault();
        }

        public static T SingleOrDefault<T>(this IEnumerable<T> sequence)
        {
            using var enumerator = sequence.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                return default!;
            }
            var result = enumerator.Current;
            if (enumerator.MoveNext())
            {
                throw new InvalidOperationException();
            }
            return result;
        }

        public static T Single<T>(this IEnumerable<T> sequence)
        {
            using var enumerator = sequence.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                throw new InvalidOperationException();
            }
            var result = enumerator.Current;
            if (enumerator.MoveNext())
            {
                throw new InvalidOperationException();
            }
            return result;
        }

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> first, IEnumerable<T> second)
        {
            foreach (var item in first)
            {
                yield return item;
            }
            foreach (var item in second)
            {
                yield return item;
            }
        }

        public static IEnumerable<T> Skip<T>(this IEnumerable<T> sequence, int skipCount)
        {
            foreach (var item in sequence)
            {
                if (skipCount <= 0)
                {
                    yield return item;
                }
                else
                {
                    --skipCount;
                }
            }
        }

        public static IEnumerable<T> SkipWhile<T>(this IEnumerable<T> sequence, Func<T, bool> predicate)
        {
            var skip = true;
            foreach (var item in sequence)
            {
                skip = skip && predicate(item);
                if (!skip)
                {
                    yield return item;
                }
            }
        }

        public static IEnumerable<T> TakeWhile<T>(this IEnumerable<T> sequence, Func<T, bool> predicate)
        {
            var take = true;
            foreach (var item in sequence)
            {
                take = take && predicate(item);
                if (take)
                {
                    yield return item;
                }
            }
        }

        public static IEnumerable<T> DefaultIfEmpty<T>(this IEnumerable<T> sequence, T defaultValue)
        {
            var isEmpty = true;
            foreach (var item in sequence)
            {
                yield return item;
                isEmpty = false;
            }
            if (isEmpty)
            {
                yield return defaultValue;
            }
        }

        public static bool Any<T>(this IEnumerable<T> sequence)
        {
            using var enumerator = sequence.GetEnumerator();
            return enumerator.MoveNext();
        }

        public static bool Any<T>(this IEnumerable<T> sequence, Func<T, bool> predicate)
        {
            return sequence.Where(predicate).Any();
        }

        public static bool All<T>(this IEnumerable<T> sequence, Func<T, bool> predicate)
        {
            foreach (var item in sequence)
            {
                if (!predicate(item))
                {
                    return false;
                }
            }
            return true;
        }

        public static int Count<T>(this IEnumerable<T> sequence)
        {
            var count = 0;
            foreach (var item in sequence)
            {
                ++count;
            }
            return count;
        }

        public static bool Contains<T>(this IEnumerable<T> sequence, T value)
        {
            foreach (var item in sequence)
            {
                if (Equals(item, value))
                {
                    return true;
                }
            }
            return false;
        }

        public static TSource Aggregate<TSource>(this IEnumerable<TSource> source, Func<TSource, TSource, TSource> func)
        {
            using var enumerator = source.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                throw new InvalidOperationException();
            }
            var accumulator = enumerator.Current;
            while (enumerator.MoveNext())
            {
                accumulator = func(accumulator, enumerator.Current);
            }
            return accumulator;
        }

        public static TAccumulate Aggregate<TSource, TAccumulate>(this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func)
        {
            var accumulator = seed;
            foreach (var item in source)
            {
                accumulator = func(accumulator, item);
            }
            return accumulator;
        }

        public static ILookup<TKey, TSource> ToLookup<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            return source.ToLookup(keySelector, e => e);
        }

        public static ILookup<TKey, TElement> ToLookup<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            var lookup = new Lookup<TKey, TElement>();
            foreach (var item in source)
            {
                lookup.Add(keySelector(item), elementSelector(item));
            }
            return lookup;
        }

        public static IEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            return source.ToLookup(keySelector);
        }

        public static IEnumerable<TResult> GroupBy<TSource, TKey, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector)
        {
            foreach (var group in source.ToLookup(keySelector))
            {
                yield return resultSelector(group.Key, group);
            }
        }

        public static Dictionary<TKey, TSource> ToDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            return source.ToDictionary(keySelector, e => e);
        }

        public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            var result = new Dictionary<TKey, TElement>();
            foreach (var item in source)
            {
                result.Add(keySelector(item), elementSelector(item));
            }
            return result;
        }
    }

    internal interface ILookup<TKey, TElement> : IEnumerable<IGrouping<TKey, TElement>>, IEnumerable
    {
        IEnumerable<TElement> this[TKey key] { get; }
        int Count { get; }
        bool Contains(TKey key);
    }

    internal interface IGrouping<out TKey, TElement> : IEnumerable<TElement>, IEnumerable
    {
        TKey Key { get; }
    }

    internal sealed class Lookup<TKey, TElement> : ILookup<TKey, TElement>
    {
        private readonly Dictionary<TKey, List<TElement>> entries = new Dictionary<TKey, List<TElement>>();
        private readonly List<TKey> keys = new List<TKey>();

        public int Count => entries.Count;

        private sealed class Grouping : IGrouping<TKey, TElement>
        {
            private readonly IEnumerable<TElement> elements;

            public TKey Key { get; }

            public Grouping(TKey key, IEnumerable<TElement> elements)
            {
                Key = key;
                this.elements = elements;
            }

            public IEnumerator<TElement> GetEnumerator() => elements.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public void Add(TKey key, TElement element)
        {
            if (!entries.TryGetValue(key, out var group))
            {
                keys.Add(key);
                group = new List<TElement>();
                entries.Add(key, group);
            }
            group.Add(element);
        }

        public IEnumerable<TElement> this[TKey key]
        {
            get
            {
                return entries.TryGetValue(key, out var elements) ? elements : Enumerable.Empty<TElement>();
            }
        }

        public bool Contains(TKey key) => entries.ContainsKey(key);

        public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator()
        {
            foreach (var key in keys)
            {
                yield return new Grouping(key, entries[key]);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
