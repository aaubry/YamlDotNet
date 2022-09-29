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
using System.Text.RegularExpressions;

namespace YamlDotNet.Core.Tokens
{
    /// <summary>
    /// Represents a tag directive token.
    /// </summary>
    public class TagDirective : Token
    {

        /// <summary>
        /// Gets the handle.
        /// </summary>
        /// <value>The handle.</value>
        public string Handle { get; }

        /// <summary>
        /// Gets the prefix.
        /// </summary>
        /// <value>The prefix.</value>
        public string Prefix { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TagDirective"/> class.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <param name="prefix">The prefix.</param>
        public TagDirective(string handle, string prefix)
            : this(handle, prefix, Mark.Empty, Mark.Empty)
        {
        }

        private static readonly Regex TagHandlePattern = new Regex(@"^!([0-9A-Za-z_\-]*!)?$", StandardRegexOptions.Compiled);

        /// <summary>
        /// Initializes a new instance of the <see cref="TagDirective"/> class.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <param name="prefix">The prefix.</param>
        /// <param name="start">The start position of the token.</param>
        /// <param name="end">The end position of the token.</param>
        public TagDirective(string handle, string prefix, Mark start, Mark end)
            : base(start, end)
        {
            if (string.IsNullOrEmpty(handle))
            {
                throw new ArgumentNullException(nameof(handle), "Tag handle must not be empty.");
            }

            if (!TagHandlePattern.IsMatch(handle))
            {
                throw new ArgumentException("Tag handle must start and end with '!' and contain alphanumerical characters only.", nameof(handle));
            }

            this.Handle = handle;

            if (string.IsNullOrEmpty(prefix))
            {
                throw new ArgumentNullException(nameof(prefix), "Tag prefix must not be empty.");
            }

            this.Prefix = prefix;
        }

        /// <summary>
        /// Determines whether the specified System.Object is equal to the current System.Object.
        /// </summary>
        /// <param name="obj">The System.Object to compare with the current System.Object.</param>
        /// <returns>
        /// true if the specified System.Object is equal to the current System.Object; otherwise, false.
        /// </returns>
        public override bool Equals(object? obj)
        {
            return obj is TagDirective other
                && Handle.Equals(other.Handle)
                && Prefix.Equals(other.Prefix);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            return Handle.GetHashCode() ^ Prefix.GetHashCode();
        }

        /// <summary/>
        public override string ToString()
        {
            return $"{Handle} => {Prefix}";
        }
    }
}
