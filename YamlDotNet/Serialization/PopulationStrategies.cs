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
using YamlDotNet.Core;

namespace YamlDotNet.Serialization
{
    /// <summary>
    /// Defines how arrays will be treated during deserialization when populating a pre-existing object (via <see cref="Deserializer.PopulateObject{T}(IParser, T)"/> and overloads)
    /// </summary>
    public enum PreexistingArrayPopulationStrategy
    {
        /// <summary>
        /// Always create a new instance.
        /// </summary>
        CreateNew,
        /// <summary>
        /// Fill the pre-existing array, overwriting items. Pre-existing values that do not get overwritten will be kept.
        /// If the pre-existing array does not have sufficient length, a <see cref="YamlException"/> will be thrown.
        /// If the array does not exist yet, an instance will be created and populated.
        /// </summary>
        FillExisting
    }

    /// <summary>
    /// Defines how types inherting from <see cref="ICollection"/> and <see cref="ICollection{T}"/> will be treated during deserialization when populating a pre-existing object (via <see cref="Deserializer.PopulateObject{T}(IParser, T)"/> and overloads)
    /// </summary>
    public enum PreexistingCollectionPopulationStrategy
    {
        /// <summary>
        /// Always create a new instance.
        /// </summary>
        CreateNew,
        /// <summary>
        /// Add items to the pre-existing collection.
        /// If the collection does not exist yet, an instance will be created and populated.
        /// </summary>
        AddItems
    }

    /// <summary>
    /// Defines how types inherting from <see cref="IDictionary"/> and <see cref="IDictionary{TKey, TValue}"/> will be treated during  deserialization when populating a pre-existing object (via <see cref="Deserializer.PopulateObject{T}(IParser, T)"/> and overloads)
    /// </summary>
    public enum PreexistingDictionaryPopulationStrategy
    {
        /// <summary>
        /// Always create a new instance.
        /// </summary>
        CreateNew,
        /// <summary>
        /// Add items to the pre-existing dictionary.
        /// If the key is already present in the pre-existing collection, an <see cref="ArgumentException"/> will be thrown.
        /// If the dictionary does not exist yet, an instance will be created and populated.
        /// </summary>
        AddItemsThrowOnExistingKeys,
        /// <summary>
        /// Add items to the pre-existing dictionary, replacing values with pre-existing keys.
        /// If the dictionary does not exist yet, an instance will be created and populated.
        /// </summary>
        AddItemsReplaceExistingKeys
    }
}
