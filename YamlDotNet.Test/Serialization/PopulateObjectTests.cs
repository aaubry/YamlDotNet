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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using FluentAssertions;
using Xunit;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace YamlDotNet.Test.Serialization
{
    public class PopulateObjectTests : SerializationTestHelper
    {
        #region Simple objects

        class SimpleClass
        {
            public int Int { get; set; }
            public string String { get; set; }
        }

        class SimpleParentClass
        {
            public int Int { get; set; }
            public string String { get; set; }
            public SimpleClass Child { get; set; }
        }

        [Fact]
        public void PopulateSimpleObject()
        {
            var target = new SimpleClass { Int = 1, String = "one" };

            var result = Deserializer.PopulateObject(@"Int: 2", target);

            Assert.Same(result, target);

            result.Int.Should().Be(2);
            result.String.Should().Be("one");
        }

        [Fact]
        public void PopulateSimpleObjectGraph()
        {
            var child = new SimpleClass { Int = 1, String = "one" };
            var target = new SimpleParentClass()
            {
                Int = 10,
                String = "ten",
                Child = child
            };

            var yaml = @"
Int: 20
Child:
    String: two";

            var result = Deserializer.PopulateObject(yaml, target);

            result.Int.Should().Be(20);

            Assert.Same(result.Child, child);

            result.Child.Int.Should().Be(1);
            result.Child.String.Should().Be("two");
        }
        #endregion

        #region Simple structs
        struct SimpleStruct
        {
            public int Int { get; set; }
            public string String { get; set; }
        }

        [Fact]
        public void PopulateSimpleStruct()
        {
            var target = new SimpleStruct { Int = 1, String = "one" };

            var result = Deserializer.PopulateObject(@"Int: 2", target);

            result.Int.Should().Be(2);
            result.String.Should().Be("one");
        }
        #endregion

        #region Populate collections
        #region Populate collections: Arrays of value types
        class IntArrayContainer
        {
            public int[] Ints { get; set; }
        }

        [Fact]
        public void PopulateArray_OfValueType_AsNestedNode_CreateNew()
        {
            var intArray = new int[] { 1, 2 };
            var container = new IntArrayContainer { Ints = intArray };

            var yaml = @"
Ints:
- 10";

            var result = DeserializerBuilder
                .WithPopulatingOptions(arrayStrategy: ArrayPopulatingStrategy.CreateNew)
                .Build()
                .PopulateObject(yaml, container);

            // original array should remain unchanged
            Assert.NotSame(result.Ints, intArray);
            intArray.ShouldBeEquivalentTo(new[] { 1, 2 });

            result.Ints.ShouldBeEquivalentTo(new[] { 10 });
        }

        [Fact]
        public void PopulateArray_OfValueType_AsNestedNode_FillExisting()
        {
            var intArray = new int[] { 1, 2 };
            var container = new IntArrayContainer { Ints = intArray };

            var yaml = @"
Ints:
- 10";

            var result = DeserializerBuilder
                .WithPopulatingOptions(arrayStrategy: ArrayPopulatingStrategy.FillExisting)
                .Build()
                .PopulateObject(yaml, container);

            Assert.Same(result.Ints, intArray);

            result.Ints.ShouldBeEquivalentTo(new[] { 10, 2 });
        }

        [Fact]
        public void PopulateArray_OfValueType_AsNestedNode_FillExisting_WithInsufficientSize()
        {
            var intArray = new int[] { 1, 2 };
            var container = new IntArrayContainer { Ints = intArray };

            var yaml = @"
Ints:
- 10
- 20
- 30";

            Action action = () => DeserializerBuilder
                  .WithPopulatingOptions(arrayStrategy: ArrayPopulatingStrategy.FillExisting)
                  .Build()
                  .PopulateObject(yaml, container);

            action.ShouldThrow<YamlException>().Where(ex => ex.Message.Contains("exceed size"));
        }

        [Fact]
        public void PopulateArray_OfValueType_AsRootNode_CreateNew()
        {
            var intArray = new int[] { 1, 2 };

            var yaml = @"- 10";

            var result = DeserializerBuilder
                .WithPopulatingOptions(arrayStrategy: ArrayPopulatingStrategy.CreateNew)
                .Build()
                .PopulateObject(yaml, intArray);

            // original array should remain unchanged
            Assert.NotSame(result, intArray);
            intArray.ShouldAllBeEquivalentTo(new[] { 1, 2 });

            result.ShouldBeEquivalentTo(new[] { 10 });
        }

        [Fact]
        public void PopulateArray_OfValueType_AsRootNode_FillExisting()
        {
            var intArray = new int[] { 1, 2 };

            var yaml = @"- 10";

            var result = DeserializerBuilder
                 .WithPopulatingOptions(arrayStrategy: ArrayPopulatingStrategy.FillExisting)
                 .Build()
                 .PopulateObject(yaml, intArray);

            Assert.Same(result, intArray);

            result.ShouldBeEquivalentTo(new[] { 10, 2 });
        }
        #endregion

        #region Populate collections: Collections (lists) of value types
        public class IntGenericCollectionContainer
        {
            public IList<int> Ints { get; set; }
        }

        [Fact]
        public void PopulateCollection_OfValueType_AsNestedNode_CreateNew()
        {
            var intCollection = new List<int> { 1, 2 };
            var container = new IntGenericCollectionContainer { Ints = intCollection };

            var yaml = @"
Ints:
- 10";

            var result = DeserializerBuilder
                .WithPopulatingOptions(collectionStrategy: CollectionPopulatingStrategy.CreateNew)
                .Build()
                .PopulateObject(yaml, container);

            Assert.NotSame(result.Ints, intCollection);
            result.Ints.ShouldBeEquivalentTo(new[] { 10 });
        }

        [Fact]
        public void PopulateCollection_OfValueType_AsNestedNode_AddItems()
        {
            var intCollection = new List<int> { 1, 2 };
            var container = new IntGenericCollectionContainer { Ints = intCollection };

            var yaml = @"
Ints:
- 10";

            var result = DeserializerBuilder
                .WithPopulatingOptions(collectionStrategy: CollectionPopulatingStrategy.AddItems)
                .Build()
                .PopulateObject(yaml, container);

            Assert.Same(result.Ints, intCollection);
            result.Ints.ShouldBeEquivalentTo(new[] { 1, 2, 10 });
        }

        [Fact]
        public void PopulateCollection_OfValueType_AsRootNode_CreateNew()
        {
            var intCollection = new List<int> { 1, 2 };

            var yaml = @"- 10";

            var result = DeserializerBuilder
                .WithPopulatingOptions(collectionStrategy: CollectionPopulatingStrategy.CreateNew)
                .Build()
                .PopulateObject(yaml, intCollection);

            Assert.NotSame(result, intCollection);
            result.ShouldBeEquivalentTo(new[] { 10 });
        }

        [Fact]
        public void PopulateCollection_OfValueType_AsRootNode_AddItems()
        {
            var intCollection = new List<int> { 1, 2 };

            var yaml = @"- 10";

            var result = DeserializerBuilder
                .WithPopulatingOptions(collectionStrategy: CollectionPopulatingStrategy.AddItems)
                .Build()
                .PopulateObject(yaml, intCollection);

            Assert.Same(result, intCollection);
            result.ShouldBeEquivalentTo(new[] { 1, 2, 10 });
        }
        #endregion

        #region Populate collections: Dictionaries of value types
        class StringIntDictionaryContainer
        {
            public Dictionary<string, int> StringInts { get; set; }
        }

        [Fact]
        public void PopulateDictionary_OfValueTypes_AsNestedNode_CreateNew()
        {
            var stringInts = new Dictionary<string, int>
            {
                { "one", 1 },
                { "two", 2 },
            };
            var container = new StringIntDictionaryContainer { StringInts = stringInts };

            var yaml = @"
StringInts:
    one: 10
    three: 30";

            var result = DeserializerBuilder
                .WithPopulatingOptions(dictionaryStrategy: DictionaryPopulatingStrategy.CreateNew)
                .Build()
                .PopulateObject(yaml, container);

            Assert.NotSame(result.StringInts, stringInts);
            container.StringInts.ShouldBeEquivalentTo(new Dictionary<string, int> {
                { "one", 10 },
                { "three", 30 }
            });
        }

        [Fact]
        public void PopulateDictionary_OfValueTypes_AsNestedNode_AddItemsReplaceExistingKeys()
        {
            var stringInts = new Dictionary<string, int>
            {
                { "one", 1 },
                { "two", 2 },
            };
            var container = new StringIntDictionaryContainer { StringInts = stringInts };

            var yaml = @"
StringInts:
    one: 10
    three: 30";

            var result = DeserializerBuilder
                .WithPopulatingOptions(dictionaryStrategy: DictionaryPopulatingStrategy.AddItemsReplaceExistingKeys)
                .Build()
                .PopulateObject(yaml, container);

            Assert.Same(result.StringInts, stringInts);
            container.StringInts.ShouldBeEquivalentTo(new Dictionary<string, int> {
                { "one", 10 },
                { "two", 2 },
                { "three", 30 }
            });
        }

        [Fact]
        public void PopulateDictionary_OfValueTypes_AsNestedNode_AddItemsThrowOnExistingKeys()
        {
            var stringInts = new Dictionary<string, int>
            {
                { "one", 1 },
                { "two", 2 },
            };
            var container = new StringIntDictionaryContainer { StringInts = stringInts };

            var yaml = @"
StringInts:
    one: 10
    three: 30";

            Action action = () => DeserializerBuilder
                .WithPopulatingOptions(dictionaryStrategy: DictionaryPopulatingStrategy.AddItemsThrowOnExistingKeys)
                .Build()
                .PopulateObject(yaml, container);

            action.ShouldThrow<YamlException>().WithInnerException<ArgumentException>().Where(ex => ex.InnerException.Message.Contains("same key"));
        }

        [Fact]
        public void PopulateDictionary_OfValueTypes_AsRootNode_CreateNew()
        {
            var stringInts = new Dictionary<string, int>
            {
                { "one", 1 },
                { "two", 2 },
            };

            var yaml = @"
one: 10
three: 30";

            var result = DeserializerBuilder
                .WithPopulatingOptions(dictionaryStrategy: DictionaryPopulatingStrategy.CreateNew)
                .Build()
                .PopulateObject(yaml, stringInts);

            Assert.NotSame(result, stringInts);
            result.ShouldBeEquivalentTo(new Dictionary<string, int> {
                { "one", 10 },
                { "three", 30 }
            });
        }

        [Fact]
        public void PopulateDictionary_OfValueTypes_AsRootNode_AddItemsReplaceExistingKeys()
        {
            var stringInts = new Dictionary<string, int>
            {
                { "one", 1 },
                { "two", 2 },
            };

            var yaml = @"
one: 10
three: 30";

            var result = DeserializerBuilder
                .WithPopulatingOptions(dictionaryStrategy: DictionaryPopulatingStrategy.AddItemsReplaceExistingKeys)
                .Build()
                .PopulateObject(yaml, stringInts);

            Assert.Same(result, stringInts);
            result.ShouldBeEquivalentTo(new Dictionary<string, int> {
                { "one", 10 },
                { "two", 2 },
                { "three", 30 }
            });
        }
        #endregion

        #region Edge cases
        class GenericListButNotNonGenericList<T> : IList<T>
        {
            public T this[int index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public int Count => throw new NotImplementedException();

            public bool IsReadOnly => throw new NotImplementedException();

            public void Add(T item)
            {
                throw new NotImplementedException();
            }

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public bool Contains(T item)
            {
                throw new NotImplementedException();
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            public IEnumerator<T> GetEnumerator()
            {
                throw new NotImplementedException();
            }

            public int IndexOf(T item)
            {
                throw new NotImplementedException();
            }

            public void Insert(int index, T item)
            {
                throw new NotImplementedException();
            }

            public bool Remove(T item)
            {
                throw new NotImplementedException();
            }

            public void RemoveAt(int index)
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void PopulateList_WithTypeNotSupportedForPopulating_CreateNew()
        {
            var list = new GenericListButNotNonGenericList<string>();

            Action action = () => DeserializerBuilder
                .WithPopulatingOptions(collectionStrategy: CollectionPopulatingStrategy.CreateNew)
                .Build()
                .PopulateObject("- one", list);

            action.ShouldThrow<YamlException>().WithInnerException<NotImplementedException>();
        }

        [Fact]
        public void PopulateList_WithTypeNotSupportedForPopulating_AddItems()
        {
            var list = new GenericListButNotNonGenericList<string>();

            Action action = () => DeserializerBuilder
                .WithPopulatingOptions(collectionStrategy: CollectionPopulatingStrategy.AddItems)
                .Build()
                .PopulateObject("- one", list);

            action.ShouldThrow<YamlException>().WithInnerException<NotSupportedException>();
        }

        class GenericDictionaryButNotNonGenericDictionary<TKey, TValue> : IDictionary<TKey, TValue>
        {
            public TValue this[TKey key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public ICollection<TKey> Keys => throw new NotImplementedException();

            public ICollection<TValue> Values => throw new NotImplementedException();

            public int Count => throw new NotImplementedException();

            public bool IsReadOnly => throw new NotImplementedException();

            public void Add(TKey key, TValue value)
            {
                throw new NotImplementedException();
            }

            public void Add(KeyValuePair<TKey, TValue> item)
            {
                throw new NotImplementedException();
            }

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public bool Contains(KeyValuePair<TKey, TValue> item)
            {
                throw new NotImplementedException();
            }

            public bool ContainsKey(TKey key)
            {
                throw new NotImplementedException();
            }

            public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
            {
                throw new NotImplementedException();
            }

            public bool Remove(TKey key)
            {
                throw new NotImplementedException();
            }

            public bool Remove(KeyValuePair<TKey, TValue> item)
            {
                throw new NotImplementedException();
            }

            public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void PopulateDictionary_WithTypeNotSupportedForPopulating_CreateNew()
        {
            var dictionary = new GenericDictionaryButNotNonGenericDictionary<string, string>();

            Action action = () => DeserializerBuilder
                    .WithPopulatingOptions(dictionaryStrategy: DictionaryPopulatingStrategy.CreateNew)
                    .Build()
                    .PopulateObject("one: ten", dictionary);

            action.ShouldThrow<YamlException>().WithInnerException<NotImplementedException>();
        }

        [Fact]
        public void PopulateDictionary_WithTypeNotSupportedForPopulating_AddItemsReplaceExistingKeys()
        {
            var dictionary = new GenericDictionaryButNotNonGenericDictionary<string, string>();

            Action action = () => DeserializerBuilder
                .WithPopulatingOptions(dictionaryStrategy: DictionaryPopulatingStrategy.AddItemsReplaceExistingKeys)
                .Build()
                .PopulateObject("one: ten", dictionary);

            action.ShouldThrow<YamlException>().WithInnerException<NotSupportedException>();
        }
        #endregion
        #endregion
    }
}
