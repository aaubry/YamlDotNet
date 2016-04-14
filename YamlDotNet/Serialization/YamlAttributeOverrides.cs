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
using System.Linq;
using System.Globalization;
using System.Collections.Generic;

namespace YamlDotNet.Serialization
{
    /// <summary>
    /// Define a collection of YamlAttribute Overrides for pre-defined object types.
    /// </summary>
    public sealed class YamlAttributeOverrides
    {
        private readonly Dictionary<Type, Dictionary<string, List<Attribute>>> overrides = new Dictionary<Type, Dictionary<string, List<Attribute>>>();
        
        public ICollection<Attribute> this[Type type, string member]
        {
        	get
        	{
        		Dictionary<string, List<Attribute>> dict;
	            if (!overrides.TryGetValue(type, out dict))
	            	return null;
	            
	            List<Attribute> list;
	            if (!dict.TryGetValue(member, out list))
	            	return null;
	            
	            return list;
        	}
        }
        
        public T GetAttribute<T>(Type type, string member) where T : Attribute
        {
       		var list = this[type, member];
       		return list == null ? null : list.OfType<T>().FirstOrDefault();
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="YamlAttributeOverrides"/> class.
        /// </summary>
        public YamlAttributeOverrides()
        {
        }
        
        /// <summary>
        /// Add a Member Attribute Override
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="member">Class Member</param>
        /// <param name="attribute">Overriding Attribute</param>
        public void Add(Type type, string member, Attribute attribute)
        {
            Dictionary<string, List<Attribute>> dict;
            if (!overrides.TryGetValue(type, out dict))
            {
                dict = new Dictionary<string, List<Attribute>>();
                overrides.Add(type, dict);
            }
            
            List<Attribute> list;
            if (!dict.TryGetValue(member, out list))
            {
            	list = new List<Attribute>();
            	dict.Add(member, list);
            }
            
            if (list.Any(attr => attr.GetType().IsInstanceOfType(attribute)))
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Attribute ({3}) already set for Type {0}, Member {1}", type.FullName, member, attribute));

            list.Add(attribute);
        }        
    }
}
