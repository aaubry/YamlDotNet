//  This file is part of YamlDotNet - A .NET library for YAML.
//  Copyright (c) 2008, 2009, 2010, 2011, 2012, 2013 Antoine Aubry
    
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
using YamlDotNet.Events;

namespace YamlDotNet.Serialization
{
	/// <summary>
	/// Allows an object to customize how it is serialized and deserialized.
	/// </summary>
	public interface IYamlSerializable
	{
	    /// <summary>
	    /// Reads this object's state from a YAML parser.
	    /// </summary>
	    /// <param name="context">The context.</param>
	    /// <param name="value"></param>
	    /// <param name="typeDescriptor"></param>
	    /// <returns>A instance of the object deserialized from Yaml.</returns>
	    ValueResult ReadYaml(SerializerContext context, object value, ITypeDescriptor typeDescriptor);

		/// <summary>
		/// Writes this object's state to a YAML emitter.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="value">The value.</param>
		/// <param name="typeDescriptor"></param>
		void WriteYaml(SerializerContext context, object value, ITypeDescriptor typeDescriptor);
	}


    /// <summary>
    /// A deserialized value.
    /// </summary>
    public struct ValueResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValueResult"/> struct.
        /// </summary>
        /// <param name="value">The value.</param>
        public ValueResult(object value) : this()
        {
            Value = value;
            IsAlias = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueResult"/> struct.
        /// </summary>
        /// <param name="isAlias">if set to <c>true</c> [is alias].</param>
        /// <param name="value">The value.</param>
        private ValueResult(bool isAlias, AnchorAlias value)
        {
            IsAlias = isAlias;
            Value = value;
        }

        /// <summary>
        /// News the alias.
        /// </summary>
        /// <param name="alias">The alias.</param>
        /// <returns>ValueResult.</returns>
        public static ValueResult NewAlias(AnchorAlias alias)
        {
            return new ValueResult(true, alias);
        }

        /// <summary>
        /// The returned value or null if no value.
        /// </summary>
        public readonly object Value;

        /// <summary>
        /// True if this value result is an alias.
        /// </summary>
        public readonly bool IsAlias;

        /// <summary>
        /// Gets the alias, only valid if <see cref="IsAlias"/> is true, null otherwise.
        /// </summary>
        /// <value>The alias.</value>
        public AnchorAlias Alias
        {
            get { return IsAlias ? (AnchorAlias)Value : null; }
        }
    }
}