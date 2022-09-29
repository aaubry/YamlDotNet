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
using YamlDotNet.Core;

namespace YamlDotNet.Serialization
{
    /// <summary>
    /// Provides special Yaml serialization instructions.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class YamlMemberAttribute : Attribute
    {
        /// <summary>
        /// Decription/Comment about this property.
        /// When set, a comment will be emitted when serializing this member.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Specifies that this property should be serialized as the given type, rather than using the actual runtime value's type.
        /// </summary>
        public Type? SerializeAs { get; set; }

        /// <summary>
        /// Specifies the order priority of this property.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Instructs the <see cref="Deserializer"/> to use a different field name for serialization.
        /// </summary>
        public string? Alias { get; set; }

        /// <summary>
        /// When false, naming conventions will not be applied to this member. Defaults to true.
        /// </summary>
        public bool ApplyNamingConventions { get; set; }

        /// <summary>
        /// Specifies the scalar style of the property when serialized. This will only affect the serialization of scalar properties.
        /// </summary>
        public ScalarStyle ScalarStyle { get; set; }

        private DefaultValuesHandling? defaultValuesHandling;

        /// <summary>
        /// Overrides how null and default values should be handled for this property.
        /// </summary>
        public DefaultValuesHandling DefaultValuesHandling
        {
            get => defaultValuesHandling.GetValueOrDefault();
            set => defaultValuesHandling = value;
        }

        public bool IsDefaultValuesHandlingSpecified => defaultValuesHandling.HasValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="YamlMemberAttribute" /> class.
        /// </summary>
        public YamlMemberAttribute()
        {
            ScalarStyle = ScalarStyle.Any;
            ApplyNamingConventions = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YamlMemberAttribute" /> class.
        /// </summary>
        /// <param name="serializeAs">Specifies that this property should be serialized as the given type, rather than using the actual runtime value's type.</param>
        public YamlMemberAttribute(Type serializeAs) : this()
        {
            SerializeAs = serializeAs ?? throw new ArgumentNullException(nameof(serializeAs));
        }
    }
}
