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

#if !NET20

using System;
using System.Reflection;
using System.Linq.Expressions;
using System.Linq;
using System.Collections.Generic;

namespace YamlDotNet.Helpers
{
    public static class ExpressionExtensions
    {
        /// <summary>
        /// Returns the <see cref="PropertyInfo" /> that describes the property that
        /// is being returned in an expression in the form:
        /// <code>
        ///   x => x.SomeProperty
        /// </code>
        /// </summary>
        public static PropertyInfo AsProperty(this LambdaExpression propertyAccessor)
        {
            var property = TryGetMemberExpression<PropertyInfo>(propertyAccessor);
            if (property == null)
            {
                throw new ArgumentException("Expected a lambda expression in the form: x => x.SomeProperty", nameof(propertyAccessor));
            }

            return property;
        }

        private static TMemberInfo? TryGetMemberExpression<TMemberInfo>(LambdaExpression lambdaExpression)
            where TMemberInfo : MemberInfo
        {
            if (lambdaExpression.Parameters.Count != 1)
            {
                return null;
            }

            var body = lambdaExpression.Body;

            if (body is UnaryExpression castExpression)
            {
                if (castExpression.NodeType != ExpressionType.Convert)
                {
                    return null;
                }

                body = castExpression.Operand;
            }

            if (body is MemberExpression memberExpression)
            {
                if (memberExpression.Expression != lambdaExpression.Parameters[0])
                {
                    return null;
                }

                return memberExpression.Member as TMemberInfo;
            }
            return null;
        }

        public static Expression Apply(this LambdaExpression expression, Expression value)
        {
            if (1 != expression.Parameters.Count)
            {
                throw new ArgumentException($"The number of values (1) must be equal to the number of parameters of the lambda expression ({expression.Parameters.Count}).", nameof(value));
            }

            var visitor = new SingleParameterReplacementVisitor(expression.Parameters[0], value);
            return visitor.Visit(expression.Body);
        }

        // TODO: Consider a special case for 2 parameters
        public static Expression Apply(this LambdaExpression expression, params Expression[] values)
        {
            if (values.Length != expression.Parameters.Count)
            {
                throw new ArgumentException($"The number of values ({values.Length}) must be equal to the number of parameters of the lambda expression ({expression.Parameters.Count}).", nameof(values));
            }

            var replacements = new Dictionary<ParameterExpression, Expression>(values.Length);
            for (int i = 0; i < values.Length; ++i)
            {
                replacements.Add(expression.Parameters[i], values[i]);
            }

            var visitor = new MultipleParametersReplacementVisitor(replacements);
            return visitor.Visit(expression.Body);
        }

        private sealed class SingleParameterReplacementVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression parameter;
            private readonly Expression value;

            public SingleParameterReplacementVisitor(ParameterExpression parameter, Expression value)
            {
                this.parameter = parameter;
                this.value = value;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return node == parameter ? value : base.VisitParameter(node);
            }
        }

        private sealed class MultipleParametersReplacementVisitor : ExpressionVisitor
        {
            private readonly Dictionary<ParameterExpression, Expression> replacements;

            public MultipleParametersReplacementVisitor(Dictionary<ParameterExpression, Expression> replacements)
            {
                this.replacements = replacements;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return replacements.TryGetValue(node, out var replacement)
                    ? replacement
                    : base.VisitParameter(node);
            }
        }
    }
}
#endif