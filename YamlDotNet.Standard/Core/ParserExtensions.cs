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

using System.Globalization;
using System.IO;
using YamlDotNet.Core.Events;

namespace YamlDotNet.Core
{
    /// <summary>
    /// Extension methods that provide useful abstractions over <see cref="IParser"/>.
    /// </summary>
    public static class ParserExtensions
    {
        /// <summary>
        /// Ensures that the current event is of the specified type, returns it and moves to the next event.
        /// </summary>
        /// <typeparam name="T">Type of the <see cref="ParsingEvent"/>.</typeparam>
        /// <returns>Returns the current event.</returns>
        /// <exception cref="YamlException">If the current event is not of the specified type.</exception>
        public static T Expect<T>(this IParser parser) where T : ParsingEvent
        {
            var expectedEvent = parser.Allow<T>();
            if (expectedEvent == null)
            {
                // TODO: Throw a better exception
                var @event = parser.Current;
                throw new YamlException(@event.Start, @event.End, string.Format(CultureInfo.InvariantCulture,
                        "Expected '{0}', got '{1}' (at {2}).", typeof(T).Name, @event.GetType().Name, @event.Start));
            }
            return expectedEvent;
        }

        /// <summary>
        /// Checks whether the current event is of the specified type.
        /// </summary>
        /// <typeparam name="T">Type of the event.</typeparam>
        /// <returns>Returns true if the current event is of type <typeparamref name="T"/>. Otherwise returns false.</returns>
        public static bool Accept<T>(this IParser parser) where T : ParsingEvent
        {
            if(parser.Current == null)
            {
                if (!parser.MoveNext())
                {
                    throw new EndOfStreamException();
                }
            }
            return parser.Current is T;
        }

        /// <summary>
        /// Checks whether the current event is of the specified type.
        /// If the event is of the specified type, returns it and moves to the next event.
        /// Otherwise retruns null.
        /// </summary>
        /// <typeparam name="T">Type of the <see cref="ParsingEvent"/>.</typeparam>
        /// <returns>Returns the current event if it is of type T; otherwise returns null.</returns>
        public static T Allow<T>(this IParser parser) where T : ParsingEvent
        {
            if (!parser.Accept<T>())
            {
                return null;
            }
            var @event = (T)parser.Current;
            parser.MoveNext();
            return @event;
        }

        /// <summary>
        /// Gets the next event without consuming it.
        /// </summary>
        /// <typeparam name="T">Type of the <see cref="ParsingEvent"/>.</typeparam>
        /// <returns>Returns the current event if it is of type T; otherwise returns null.</returns>
        public static T Peek<T>(this IParser parser) where T : ParsingEvent
        {
            if (!parser.Accept<T>())
            {
                return null;
            }
            return (T)parser.Current;
        }

        /// <summary>
        /// Skips the current event and any nested event.
        /// </summary>
        public static void SkipThisAndNestedEvents(this IParser parser)
        {
            var depth = 0;
            do
            {
                depth += parser.Peek<ParsingEvent>().NestingIncrease;
                parser.MoveNext();
            }
            while (depth > 0);
        }
    }
}