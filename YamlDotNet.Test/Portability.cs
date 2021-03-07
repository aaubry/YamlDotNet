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
using System.Linq.Expressions;
using System.Reflection;
using YamlDotNet.Serialization;

namespace YamlDotNet.Test
{
#if NET20
    internal static class Portability
    {
        public static SerializerBuilder WithAttributeOverride<TClass>(this SerializerBuilder builder, Expression<Func<TClass, object>> propertyAccessor, Attribute attribute)
        {
            return builder.WithAttributeOverride(typeof(TClass), propertyAccessor.AsProperty().Name, attribute);
        }

        public static DeserializerBuilder WithAttributeOverride<TClass>(this DeserializerBuilder builder, Expression<Func<TClass, object>> propertyAccessor, Attribute attribute)
        {
            return builder.WithAttributeOverride(typeof(TClass), propertyAccessor.AsProperty().Name, attribute);
        }

        public static void Add<TClass>(this YamlAttributeOverrides overrides, Expression<Func<TClass, object>> propertyAccessor, Attribute attribute)
        {
            var property = propertyAccessor.AsProperty();
            overrides.Add(typeof(TClass), property.Name, attribute);
        }

        public static PropertyInfo AsProperty(this LambdaExpression propertyAccessor)
        {
            var property = TryGetMemberExpression<PropertyInfo>(propertyAccessor);
            if (property == null)
            {
                throw new ArgumentException("Expected a lambda expression in the form: x => x.SomeProperty", nameof(propertyAccessor));
            }

            return property;
        }

        private static TMemberInfo TryGetMemberExpression<TMemberInfo>(LambdaExpression lambdaExpression)
            where TMemberInfo : MemberInfo
        {
            if (lambdaExpression.Parameters.Count != 1)
            {
                return null;
            }

            var body = lambdaExpression.Body;

            var castExpression = body as UnaryExpression;
            if (castExpression != null)
            {
                if (castExpression.NodeType != ExpressionType.Convert)
                {
                    return null;
                }

                body = castExpression.Operand;
            }

            var memberExpression = body as MemberExpression;
            if (memberExpression == null)
            {
                return null;
            }

            if (memberExpression.Expression != lambdaExpression.Parameters[0])
            {
                return null;
            }

            return memberExpression.Member as TMemberInfo;
        }
    }
#endif
}
