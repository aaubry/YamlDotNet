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
using System.Diagnostics;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace YamlDotNet.Serialization
{
    /// <summary>
    /// An object that contains part of a YAML stream.
    /// </summary>
    public sealed class StreamFragment : IYamlConvertible
    {
        private readonly List<ParsingEvent> events = new List<ParsingEvent>();

        /// <summary>
        /// Gets or sets the events.
        /// </summary>
        /// <value>The events.</value>
        public IList<ParsingEvent> Events
        {
            get
            {
                return events;
            }
        }

        #region IYamlConvertible Members
        /// <summary>
        /// Reads this object's state from a YAML parser.
        /// </summary>
        void IYamlConvertible.Read(IParser parser, Type expectedType, ObjectDeserializer nestedObjectDeserializer)
        {
            events.Clear();

            var depth = 0;
            do
            {
                if (!parser.MoveNext())
                {
                    throw new InvalidOperationException("The parser has reached the end before deserialization completed.");
                }

                var current = parser.Current!;
                events.Add(current);
                depth += current.NestingIncrease;
            } while (depth > 0);

            Debug.Assert(depth == 0);
        }

        /// <summary>
        /// Writes this object's state to a YAML emitter.
        /// </summary>
        void IYamlConvertible.Write(IEmitter emitter, ObjectSerializer nestedObjectSerializer)
        {
            foreach (var item in events)
            {
                emitter.Emit(item);
            }
        }
        #endregion
    }
}
