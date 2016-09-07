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
using System.Linq;
using YamlDotNet.Core;

namespace YamlDotNet.Serialization
{
    public sealed class EmissionPhaseObjectGraphVisitorArgs
    {
        /// <summary>
        /// Gets the next visitor that should be called by the current visitor.
        /// </summary>
        public IObjectGraphVisitor InnerVisitor { get; private set; }

        /// <summary>
        /// Gets the <see cref="IEmitter" /> that is to be used for serialization.
        /// </summary>
        public IEmitter Emitter { get; private set; }

        /// <summary>
        /// Gets the <see cref="IEventEmitter" /> that is to be used for serialization.
        /// </summary>
        public IEventEmitter EventEmitter { get; private set; }

        private readonly IEnumerable<IObjectGraphVisitor> preProcessingPhaseVisitors;

        public EmissionPhaseObjectGraphVisitorArgs(IObjectGraphVisitor innerVisitor, IEmitter emitter, IEventEmitter eventEmitter, IEnumerable<IObjectGraphVisitor> preProcessingPhaseVisitors)
        {
            if (innerVisitor == null)
            {
                throw new ArgumentNullException("innerVisitor");
            }

            InnerVisitor = innerVisitor;

            if (emitter == null)
            {
                throw new ArgumentNullException("emitter");
            }

            Emitter = emitter;

            if (eventEmitter == null)
            {
                throw new ArgumentNullException("eventEmitter");
            }

            EventEmitter = eventEmitter;

            if (preProcessingPhaseVisitors == null)
            {
                throw new ArgumentNullException("preProcessingPhaseVisitors");
            }

            this.preProcessingPhaseVisitors = preProcessingPhaseVisitors;
        }

        /// <summary>
        /// Gets the visitor of type <typeparamref name="T" /> that was used during the pre-processig phase.
        /// </summary>
        /// <typeparam name="T">The type of the visitor.s</typeparam>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">
        /// No visitor of that type has been registered,
        /// or ore than one visitors registered are of type <typeparamref name="T"/>.
        /// </exception>
        public T GetPreProcessingPhaseObjectGraphVisitor<T>()
            where T : IObjectGraphVisitor
        {
            return preProcessingPhaseVisitors
                .OfType<T>()
                .Single();
        }
    }
}