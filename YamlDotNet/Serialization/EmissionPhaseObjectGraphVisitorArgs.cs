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
using System.Linq;
using YamlDotNet.Core;

namespace YamlDotNet.Serialization
{
    public sealed class EmissionPhaseObjectGraphVisitorArgs
    {
        /// <summary>
        /// Gets the next visitor that should be called by the current visitor.
        /// </summary>
        public IObjectGraphVisitor<IEmitter> InnerVisitor { get; private set; }

        /// <summary>
        /// Gets the <see cref="IEventEmitter" /> that is to be used for serialization.
        /// </summary>
        public IEventEmitter EventEmitter { get; private set; }

        /// <summary>
        /// Gets a function that, when called, serializes the specified object.
        /// </summary>
        public ObjectSerializer NestedObjectSerializer { get; private set; }

        public IEnumerable<IYamlTypeConverter> TypeConverters { get; private set; }

        private readonly IEnumerable<IObjectGraphVisitor<Nothing>> preProcessingPhaseVisitors;

        public EmissionPhaseObjectGraphVisitorArgs(
            IObjectGraphVisitor<IEmitter> innerVisitor,
            IEventEmitter eventEmitter,
            IEnumerable<IObjectGraphVisitor<Nothing>> preProcessingPhaseVisitors,
            IEnumerable<IYamlTypeConverter> typeConverters,
            ObjectSerializer nestedObjectSerializer
        )
        {
            InnerVisitor = innerVisitor ?? throw new ArgumentNullException(nameof(innerVisitor));
            EventEmitter = eventEmitter ?? throw new ArgumentNullException(nameof(eventEmitter));
            this.preProcessingPhaseVisitors = preProcessingPhaseVisitors ?? throw new ArgumentNullException(nameof(preProcessingPhaseVisitors));
            TypeConverters = typeConverters ?? throw new ArgumentNullException(nameof(typeConverters));
            NestedObjectSerializer = nestedObjectSerializer ?? throw new ArgumentNullException(nameof(nestedObjectSerializer));
        }

        /// <summary>
        /// Gets the visitor of type <typeparamref name="T" /> that was used during the pre-processing phase.
        /// </summary>
        /// <typeparam name="T">The type of the visitor.s</typeparam>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">
        /// No visitor of that type has been registered,
        /// or ore than one visitors registered are of type <typeparamref name="T"/>.
        /// </exception>
        public T GetPreProcessingPhaseObjectGraphVisitor<T>()
            where T : IObjectGraphVisitor<Nothing>
        {
            return preProcessingPhaseVisitors
                .OfType<T>()
                .Single();
        }
    }
}
