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

namespace YamlDotNet.Core
{
    public struct TagName : IEquatable<TagName>
    {
        public static readonly TagName Empty = default;
        public static readonly TagName NonSpecific = new TagName("!");

        private const string emptyTag = "?";

        private readonly string? value;

        public string Value => value ?? emptyTag;

        public bool IsEmpty => value is null;
        public bool IsNonSpecific => IsEmpty || Equals(NonSpecific);

        public bool IsLocal => !(value is null) && value.Length > 1 && value[0] == '!';
        public bool IsGlobal => !IsEmpty && !IsLocal;

        public TagName(string value)
        {
            this.value = value ?? throw new ArgumentNullException(nameof(value));

            if (value.Length == 0)
            {
                throw new ArgumentException("Tag value must not be empty", nameof(value));
            }

            if (IsGlobal && !Uri.IsWellFormedUriString(value, UriKind.RelativeOrAbsolute))
            {
                throw new ArgumentException("Global tags must be valid URIs", nameof(value));
            }
        }

        public override string ToString() => Value;

        public bool Equals(TagName other)
        {
            // The 'IsImplicit' property is not compared because that's not relevant
            // for tag equivalence.
            return Equals(value, other.value);
        }

        public override bool Equals(object? obj)
        {
            return obj is TagName other && Equals(other);
        }

        public override int GetHashCode()
        {
            return value?.GetHashCode() ?? 0;
        }

        public static bool operator ==(TagName left, TagName right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TagName left, TagName right)
        {
            return !(left == right);
        }

        public static bool operator ==(TagName left, string right)
        {
            return Equals(left.value, right);
        }

        public static bool operator !=(TagName left, string right)
        {
            return !(left == right);
        }

        public static implicit operator TagName(string value) => value == null ? Empty : new TagName(value);
    }
}