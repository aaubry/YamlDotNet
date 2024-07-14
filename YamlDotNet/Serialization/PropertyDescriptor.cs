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
    public sealed class PropertyDescriptor : IPropertyDescriptor
    {
        private readonly IPropertyDescriptor baseDescriptor;
        public PropertyDescriptor(IPropertyDescriptor baseDescriptor)
        {
            this.baseDescriptor = baseDescriptor;
            Name = baseDescriptor.Name;
        }

        public bool AllowNulls
        {
            get
            {
                return baseDescriptor.AllowNulls;
            }
        }

        public string Name { get; set; }

        public bool Required { get => baseDescriptor.Required; }

        public Type Type { get { return baseDescriptor.Type; } }

        public Type? TypeOverride
        {
            get { return baseDescriptor.TypeOverride; }
            set { baseDescriptor.TypeOverride = value; }
        }

        public Type? ConverterType => baseDescriptor.ConverterType;

        public int Order { get; set; }

        public ScalarStyle ScalarStyle
        {
            get { return baseDescriptor.ScalarStyle; }
            set { baseDescriptor.ScalarStyle = value; }
        }

        public bool CanWrite
        {
            get { return baseDescriptor.CanWrite; }
        }

        public void Write(object target, object? value)
        {
            baseDescriptor.Write(target, value);
        }

        public T? GetCustomAttribute<T>() where T : Attribute
        {
            return baseDescriptor.GetCustomAttribute<T>();
        }

        public IObjectDescriptor Read(object target)
        {
            return baseDescriptor.Read(target);
        }
    }
}
