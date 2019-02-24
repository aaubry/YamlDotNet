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
#if !(NET20 || NET35)
#define USE_CONCURRENT_DICTIONARY
#endif
using System;
using System.Collections.Generic;

namespace YamlDotNet.Serialization.TypeInspectors
{
    /// <summary>
    /// Wraps another <see cref="ITypeInspector"/> and applies caching.
    /// </summary>
    public sealed class CachedTypeInspector : TypeInspectorSkeleton
    {
        private readonly ITypeInspector innerTypeDescriptor;
        private readonly IDictionary<Type, List<IPropertyDescriptor>> cache;

        public CachedTypeInspector(ITypeInspector innerTypeDescriptor)
        {
            if (innerTypeDescriptor == null)
            {
                throw new ArgumentNullException("innerTypeDescriptor");
            }
#if USE_CONCURRENT_DICTIONARY
            cache = new System.Collections.Concurrent.ConcurrentDictionary<Type, List<IPropertyDescriptor>>();            
#else
            cache = new Dictionary<Type, List<IPropertyDescriptor>>();
#endif
            this.innerTypeDescriptor = innerTypeDescriptor;
        }

        public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object container)
        {
            List<IPropertyDescriptor> list;
#if USE_CONCURRENT_DICTIONARY
            if (!cache.TryGetValue(type, out list))
            {
                list = new List<IPropertyDescriptor>(innerTypeDescriptor.GetProperties(type, container));
                cache[type] = list;
            }
#else
            if (!cache.TryGetValue(type, out list))
            {
                //if not able to get the properties, lock the type 
                //this allows other threads to continue for already known types or new types
                lock (type)
                {
                    //check again because, if there was a lock contention or if the 
                    //non-concurrent dictionary was in the process of updating, it will already be done                    
                    if (!cache.TryGetValue(type, out list))
                    {
                        list = new List<IPropertyDescriptor>(innerTypeDescriptor.GetProperties(type, container));
                        cache[type] = list;
                    }
                }
            }
#endif
            return list;
        }
    }
}
