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
using YamlDotNet.Core;

namespace YamlDotNet.Serialization.Schemas
{
    public sealed class CompositeTagNameResolver : ITagNameResolver
    {
        private readonly IEnumerable<ITagNameResolver> resolvers;

        public CompositeTagNameResolver(params ITagNameResolver[] resolvers)
            : this((IEnumerable<ITagNameResolver>)resolvers)
        {
        }

        public CompositeTagNameResolver(IEnumerable<ITagNameResolver> resolvers)
        {
            this.resolvers = resolvers ?? throw new ArgumentNullException(nameof(resolvers));
        }

        public bool Resolve(Type type, out TagName tag)
        {
            foreach (var resolver in resolvers)
            {
                if (resolver.Resolve(type, out tag))
                {
                    return true;
                }
            }

            tag = default;
            return false;
        }
    }
}
