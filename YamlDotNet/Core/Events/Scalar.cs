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

namespace YamlDotNet.Core.Events
{
    /// <summary>
    /// Represents a scalar event.
    /// </summary>
    public sealed class Scalar : NodeEvent, IScalar
    {
        /// <summary>
        /// Gets the event type, which allows for simpler type comparisons.
        /// </summary>
        internal override EventType Type => EventType.Scalar;

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>The value.</value>
        public string Value { get; }

        /// <summary>
        /// Gets the style of the scalar.
        /// </summary>
        /// <value>The style.</value>
        public ScalarStyle Style { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Scalar"/> class.
        /// </summary>
        /// <param name="anchor">The anchor.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="value">The value.</param>
        /// <param name="style">The style.</param>
        /// <param name="start">The start position of the event.</param>
        /// <param name="end">The end position of the event.</param>
        public Scalar(AnchorName anchor, TagName tag, string value, ScalarStyle style, Mark start, Mark end)
            : base(anchor, tag, start, end)
        {
            this.Value = value;
            this.Style = style;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Scalar"/> class.
        /// </summary>
        /// <param name="anchor">The anchor.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="value">The value.</param>
        /// <param name="style">The style.</param>
        public Scalar(AnchorName anchor, TagName tag, string value, ScalarStyle style)
            : this(anchor, tag, value, style, Mark.Empty, Mark.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Scalar"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public Scalar(string value)
            : this(AnchorName.Empty, TagName.Empty, value, ScalarStyle.Any, Mark.Empty, Mark.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Scalar"/> class.
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <param name="value">The value.</param>
        public Scalar(TagName tag, string value)
            : this(AnchorName.Empty, tag, value, ScalarStyle.Any, Mark.Empty, Mark.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Scalar"/> class.
        /// </summary>
        public Scalar(AnchorName anchor, TagName tag, string value)
            : this(anchor, tag, value, ScalarStyle.Any, Mark.Empty, Mark.Empty)
        {
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return $"Scalar [anchor = {Anchor}, tag = {Tag}, value = {Value}, style = {Style}]";
        }

        /// <summary>
        /// Invokes run-time type specific Visit() method of the specified visitor.
        /// </summary>
        /// <param name="visitor">visitor, may not be null.</param>
        public override T Accept<T>(IParsingEventVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override NodeKind Kind => NodeKind.Scalar;
    }
}
