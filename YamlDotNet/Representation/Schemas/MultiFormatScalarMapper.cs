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
using System.Text.RegularExpressions;
using YamlDotNet.Core;

namespace YamlDotNet.Representation.Schemas
{
    public sealed class MultiFormatScalarMapper<TFormat> : INodeMapper
        where TFormat : Enum
    {
        private readonly FormatMapperEntry[] formats;

        /// <param name="tag">The tag that this mapper handles.</param>
        /// <param name="formats">
        /// A list of regular expressions and associated mappers.
        /// It is assumed that the first mapper uses the canonical representation.
        /// At least one element must be provided.
        /// </param>
        public MultiFormatScalarMapper(TagName tag, params MultiFormatScalarMapperFormat<TFormat>[] formats)
            : this(tag, (IEnumerable<MultiFormatScalarMapperFormat<TFormat>>)formats)
        {
        }

        /// <param name="tag">The tag that this mapper handles.</param>
        /// <param name="formats">
        /// A list of regular expressions and associated mappers.
        /// It is assumed that the first mapper uses the canonical representation.
        /// At least one element must be provided.
        /// </param>
        public MultiFormatScalarMapper(TagName tag, IEnumerable<MultiFormatScalarMapperFormat<TFormat>> formats)
        {
            Tag = tag;

            if (formats is null)
            {
                throw new ArgumentNullException(nameof(formats));
            }

            this.formats = formats
                .Select(f => new FormatMapperEntry(f.Pattern, f.Format, NodeMapper.CreateScalarMapper(tag, f.Constructor, f.Representer, this)))
                .ToArray();

            if (this.formats.Length == 0)
            {
                throw new ArgumentException("At least one format must be provided", nameof(formats));
            }
        }


        public IEnumerable<FormatMapperEntry> Formats => formats;

        public INodeMapper GetFormat(TFormat format)
        {
            foreach (var f in formats)
            {
                if (f.Format.Equals(format))
                {
                    return f.Mapper;
                }
            }

            throw new ArgumentException($"Invalid format '{format}'.");
        }

        public TagName Tag { get; }
        public INodeMapper Canonical => this;

        public object? Construct(Node node)
        {
            var value = node.Expect<Scalar>().Value;
            foreach (var format in Formats)
            {
                if (format.Pattern.IsMatch(value))
                {
                    return format.Mapper.Construct(node);
                }
            }

            throw new SemanticErrorException(node.Start, node.End, $"The node '{node}' could not be parsed as '{Tag}'.");
        }

        public Node Represent(object? native, ISchemaIterator iterator, IRepresentationState state)
        {
            // It is assumed that the first mapper uses the canonical representation.
            return formats[0].Mapper.Represent(native, iterator, state);
        }

        public sealed class FormatMapperEntry
        {
            public FormatMapperEntry(Regex pattern, TFormat format, INodeMapper mapper)
            {
                Pattern = pattern;
                Format = format;
                Mapper = mapper;
            }

            public Regex Pattern { get; }
            public TFormat Format { get; }
            public INodeMapper Mapper { get; }
        }
    }

    public sealed class MultiFormatScalarMapperFormat<TFormat>
    {
        public MultiFormatScalarMapperFormat(TFormat format, string pattern, Func<Scalar, object?> constructor, Func<object?, string> representer)
            : this(format, new Regex(pattern, StandardRegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture), constructor, representer)
        {
        }

        public MultiFormatScalarMapperFormat(TFormat format, Regex pattern, Func<Scalar, object?> constructor, Func<object?, string> representer)
        {
            Format = format;
            Pattern = pattern;
            Constructor = constructor;
            Representer = representer;
        }

        public TFormat Format { get; }
        public Regex Pattern { get; }
        public Func<Scalar, object?> Constructor { get; }
        public Func<object?, string> Representer { get; }
    }
}
