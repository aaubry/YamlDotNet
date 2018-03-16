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
using System.Reflection;
using YamlDotNet.Core;

namespace YamlDotNet.Serialization.TypeInspectors
{
    /// <summary>
    /// Returns the properties of a type that are readable.
    /// </summary>
    public sealed class ReadablePropertiesTypeInspector : TypeInspectorSkeleton
    {
        private readonly ITypeResolver _typeResolver;

        public ReadablePropertiesTypeInspector(ITypeResolver typeResolver)
        {
            if (typeResolver == null)
            {
                throw new ArgumentNullException("typeResolver");
            }

            _typeResolver = typeResolver;
        }

        private static bool IsValidProperty(PropertyInfo property)
        {
            return property.CanRead
                && property.GetGetMethod().GetParameters().Length == 0;
        }

        public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object container)
        {
            var properties = type
                .GetPublicProperties()
                .Where(IsValidProperty)
                .Select(p => (IPropertyDescriptor)new ReflectionPropertyDescriptor(p, _typeResolver));
            
            var fields = type
                .GetPublicFields()
                .Select(f => (IPropertyDescriptor)new ReflectionFieldDescriptor(f, _typeResolver));

            return properties.Concat(fields);
        }

        private abstract class ReflectionMemberDescriptor : IPropertyDescriptor
        {
            protected readonly MemberInfo _memberInfo;
            protected readonly ITypeResolver _typeResolver;

            public ReflectionMemberDescriptor(MemberInfo memberInfo, ITypeResolver typeResolver)
            {
                _memberInfo = memberInfo;
                _typeResolver = typeResolver;
                ScalarStyle = ScalarStyle.Any;
            }

            public string Name { get { return _memberInfo.Name; } }
            public abstract Type Type { get; }
            public Type TypeOverride { get; set; }
            public int Order { get; set; }
            public abstract bool CanWrite { get; }
            public ScalarStyle ScalarStyle { get; set; }

            public abstract void Write(object target, object value);
            protected abstract object GetValue(object target);

            public T GetCustomAttribute<T>() where T : Attribute
            {
                var attributes = _memberInfo.GetCustomAttributes(typeof(T), true);
                return (T)attributes.FirstOrDefault();
            }

            public IObjectDescriptor Read(object target)
            {
                var propertyValue = GetValue(target);
                var actualType = TypeOverride ?? _typeResolver.Resolve(Type, propertyValue);
                return new ObjectDescriptor(propertyValue, actualType, Type, ScalarStyle);
            }
        }

        private sealed class ReflectionPropertyDescriptor : ReflectionMemberDescriptor
        {
            public ReflectionPropertyDescriptor(PropertyInfo memberInfo, ITypeResolver typeResolver)
                : base(memberInfo, typeResolver)
            {
            }

            private PropertyInfo PropertyInfo { get { return ((PropertyInfo)_memberInfo); } }
            public override Type Type { get { return PropertyInfo.PropertyType; } }
            public override bool CanWrite { get { return PropertyInfo.CanWrite; } }

			protected override object GetValue(object target)
			{
                return PropertyInfo.ReadValue(target);
			}

			public override void Write(object target, object value)
            {
                PropertyInfo.SetValue(target, value, null);
            }

        }

        private sealed class ReflectionFieldDescriptor : ReflectionMemberDescriptor
        {
            public ReflectionFieldDescriptor(FieldInfo memberInfo, ITypeResolver typeResolver)
                : base(memberInfo, typeResolver)
            {
            }

            private FieldInfo FieldInfo { get { return ((FieldInfo)_memberInfo); } }
            public override Type Type { get { return FieldInfo.FieldType; } }
            public override bool CanWrite { get { return true; } }

            protected override object GetValue(object target)
            {
                return FieldInfo.GetValue(target);
            }

            public override void Write(object target, object value)
            {
                FieldInfo.SetValue(target, value);
            }
        }

    }
}
