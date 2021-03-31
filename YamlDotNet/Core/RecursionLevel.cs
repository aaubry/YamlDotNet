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
    /// Keeps track of the <see cref="current"/> recursion level,
    /// and throws <see cref="MaximumRecursionLevelReachedException"/>
    /// whenever <see cref="Maximum"/> is reached.
    /// </summary>
    internal sealed class RecursionLevel
    {
        private int current;
        public int Maximum { get; }

        public RecursionLevel(int maximum)
        {
            Maximum = maximum;
        }

        /// <summary>
        /// Increments the <see cref="current"/> recursion level,
        /// and throws <see cref="MaximumRecursionLevelReachedException"/>
        /// if <see cref="Maximum"/> is reached.
        /// </summary>
        public void Increment()
        {
            if (!TryIncrement())
            {
                throw new MaximumRecursionLevelReachedException("Maximum level of recursion reached");
            }
        }

        /// <summary>
        /// Increments the <see cref="current"/> recursion level,
        /// and returns whether <see cref="current"/> is still less than <see cref="Maximum"/>.
        /// </summary>
        public bool TryIncrement()
        {
            if (current < Maximum)
            {
                ++current;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Decrements the <see cref="current"/> recursion level.
        /// </summary>
        public void Decrement()
        {
            if (current == 0)
            {
                throw new InvalidOperationException("Attempted to decrement RecursionLevel to a negative value");
            }
            --current;
        }
    }
}
