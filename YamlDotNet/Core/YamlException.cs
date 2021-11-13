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

namespace YamlDotNet.Core
{
    /// <summary>
    /// Base exception that is thrown when the a problem occurs in the YamlDotNet library.
    /// </summary>
    public class YamlException : Exception
    {
        /// <summary>
        /// Gets the position in the input stream where the event that originated the exception starts.
        /// </summary>
        public Mark Start { get; }

        /// <summary>
        /// Gets the position in the input stream where the event that originated the exception ends.
        /// </summary>
        public Mark End { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="YamlException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public YamlException(string message)
            : this(Mark.Empty, Mark.Empty, message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YamlException"/> class.
        /// </summary>
        public YamlException(in Mark start, in Mark end, string message)
            : this(start, end, message, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YamlException"/> class.
        /// </summary>
        public YamlException(in Mark start, in Mark end, string message, Exception? innerException)
            : base(message, innerException)
        {
            Start = start;
            End = end;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YamlException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        public YamlException(string message, Exception inner)
            : this(Mark.Empty, Mark.Empty, message, inner)
        {
        }

        public override string ToString()
        {
            return $"({Start}) - ({End}): {Message}";
        }
    }
}
