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
using System.Diagnostics.CodeAnalysis;
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
        public static T Consume<T>(this IParser parser) where T : ParsingEvent
        {
            var required = parser.Require<T>();
            parser.MoveNext();
            return required;
        }

        /// <summary>
        /// Checks whether the current event is of the specified type.
        /// If the event is of the specified type, returns it and moves to the next event.
        /// Otherwise returns null.
        /// </summary>
        /// <typeparam name="T">Type of the <see cref="ParsingEvent"/>.</typeparam>
        /// <returns>Returns true if the current event is of type T; otherwise returns null.</returns>
        public static bool TryConsume<T>(this IParser parser, [MaybeNullWhen(false)] out T @event) where T : ParsingEvent
        {
            if (parser.Accept(out @event!))
            {
                parser.MoveNext();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Enforces that the current event is of the specified type.
        /// </summary>
        /// <typeparam name="T">Type of the <see cref="ParsingEvent"/>.</typeparam>
        /// <returns>Returns the current event.</returns>
        /// <exception cref="YamlException">If the current event is not of the specified type.</exception>
        public static T Require<T>(this IParser parser) where T : ParsingEvent
        {
            if (!parser.Accept<T>(out var required))
            {
                var @event = parser.Current;
                if (@event == null)
                {
                    throw new YamlException($"Expected '{typeof(T).Name}', got nothing.");
                }
                throw new YamlException(@event.Start, @event.End, $"Expected '{typeof(T).Name}', got '{@event.GetType().Name}' (at {@event.Start}).");
            }
            return required;
        }

        /// <summary>
        /// Checks whether the current event is of the specified type.
        /// </summary>
        /// <typeparam name="T">Type of the event.</typeparam>
        /// <returns>Returns true if the current event is of type <typeparamref name="T"/>. Otherwise returns false.</returns>
        public static bool Accept<T>(this IParser parser, [MaybeNullWhen(false)] out T @event) where T : ParsingEvent
        {
            if (parser.Current == null)
            {
                if (!parser.MoveNext())
                {
                    throw new EndOfStreamException();
                }
            }

            if (parser.Current is T evt)
            {
                @event = evt;
                return true;
            }
            else
            {
                @event = default!;
                return false;
            }
        }

        /// <summary>
        /// Skips the current event and any nested event.
        /// </summary>
        public static void SkipThisAndNestedEvents(this IParser parser)
        {
            var depth = 0;
            do
            {
                var next = parser.Consume<ParsingEvent>();
                depth += next.NestingIncrease;
            }
            while (depth > 0);
        }

        [Obsolete("Please use Consume<T>() instead")]
        public static T Expect<T>(this IParser parser) where T : ParsingEvent
        {
            return parser.Consume<T>();
        }

        [Obsolete("Please use TryConsume<T>(out var evt) instead")]
        [return: MaybeNull]
        public static T? Allow<T>(this IParser parser) where T : ParsingEvent
        {
            return parser.TryConsume<T>(out var @event) ? @event : default;
        }

        [Obsolete("Please use Accept<T>(out var evt) instead")]
        [return: MaybeNull]
        public static T? Peek<T>(this IParser parser) where T : ParsingEvent
        {
            return parser.Accept<T>(out var @event) ? @event : default;
        }

        [Obsolete("Please use TryConsume<T>(out var evt) or Accept<T>(out var evt) instead")]
        public static bool Accept<T>(this IParser parser) where T : ParsingEvent
        {
            return Accept<T>(parser, out var _);
        }
    }
}
