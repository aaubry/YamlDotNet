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

        public static Expression UpCast(this Expression expression, Type targetType)
        {
            if (expression.Type == targetType)
            {
                return expression;
            }

            if (!targetType.IsAssignableFrom(expression.Type))
            {
                throw new ArgumentException($"Type '{targetType.FullName}' cannot be assigned from '{expression.Type.FullName}'.");
            }

            return Expression.Convert(expression, targetType);
        }

        public static Expr<TResult> Apply<T1, TResult>(this Expression<Func<T1, TResult>> expression, Expr<T1> value)
        {
            return Apply((LambdaExpression)expression, value).As<TResult>();
        }

        public static Expr<TResult> Apply<T1, T2, TResult>(this Expression<Func<T1, T2, TResult>> expression, Expr<T1> value1, Expr<T2> value2)
        {
            return Apply((LambdaExpression)expression, value1, value2).As<TResult>();
        }

        public static Expression Apply(this LambdaExpression expression, Expression value)
        {
            if (1 != expression.Parameters.Count)
            {
                throw new ArgumentException($"The number of values (1) must be equal to the number of parameters of the lambda expression ({expression.Parameters.Count}).", nameof(value));
            }

            var parameter = expression.Parameters[0];
            var visitor = new SingleParameterReplacementVisitor(parameter, value);
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
                var parameter = expression.Parameters[i];
                replacements.Add(parameter, values[i]);
            }

            var visitor = new MultipleParametersReplacementVisitor(replacements);
            return visitor.Visit(expression.Body);
        }

        public static Expr<T> Inject<T>(this Expression<Func<T>> expression)
        {
            return ExpressionInjectorVisitor.Instance.Visit(expression.Body).As<T>();
        }

        public static Expression Inject(this Expression expression)
        {
            return ExpressionInjectorVisitor.Instance.Visit(expression);
        }

#if NET45
        public static Delegate Compile(this LambdaExpression expression, bool preferInterpretation) => expression.Compile();
#endif

        private sealed class ExpressionInjectorVisitor : ExpressionVisitor
        {
            public static readonly ExpressionInjectorVisitor Instance = new ExpressionInjectorVisitor();

            protected override Expression VisitMethodCall(MethodCallExpression originalNode)
            {
                var updatedNode = (MethodCallExpression)base.VisitMethodCall(originalNode);

                if (updatedNode.Method.IsStatic && updatedNode.Method.DeclaringType == typeof(ExpressionBuilder))
                {
                    switch (updatedNode.Method.Name)
                    {
                        case nameof(ExpressionBuilder.Inject) when updatedNode.Arguments.Count == 1:
                            return EvaluateConstant(updatedNode.Arguments[0]);

                        case nameof(ExpressionBuilder.Inject):
                            if (!(updatedNode.Arguments[0] is LambdaExpression expression))
                            {
                                expression = (LambdaExpression)EvaluateConstant(updatedNode.Arguments[0]);
                            }

                            var values = new Expression[updatedNode.Arguments.Count - 1];
                            for (int i = 0; i < values.Length; ++i)
                            {
                                values[i] = updatedNode.Arguments[i + 1];
                            }
                            return expression.Apply(values);

                        case nameof(ExpressionBuilder.Wrap):
                            var exprCtor = updatedNode.Method.ReturnType.GetConstructor(new[] { typeof(Expression) });
                            return Expression.New(exprCtor, Expression.Constant(updatedNode.Arguments[0], typeof(Expression)));

                        default:
                            throw new NotSupportedException(updatedNode.Method.Name);
                    }
                }

                return updatedNode;
            }

            private static Expression EvaluateConstant(Expression expression)
            {
                if (expression.Type.IsGenericType && expression.Type.GetGenericTypeDefinition() == typeof(Expr<>))
                {
                    expression = Expression.Field(expression, nameof(Expr<object>.Expression));
                }
                var lambda = Expression.Lambda(expression);
                return (Expression)lambda.Compile(true).DynamicInvoke()!;
            }
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

        public static Expr<T> As<T>(this Expression expression) => new Expr<T>(expression);
    }

    public static class ExpressionBuilder
    {
        public static TValue Inject<TValue>(Expression expression) => throw new NotSupportedException("Not to be called directly");
        public static TValue Inject<TValue>(Expr<TValue> expression) => throw new NotSupportedException("Not to be called directly");
        public static TResult Inject<T1, TResult>(Expression<Func<T1, TResult>> expression, T1 arg1) => throw new NotSupportedException("Not to be called directly");

        public static Expr<T> Wrap<T>(T value) => throw new NotSupportedException("Not to be called directly");
    }

    public struct Expr<T>
    {
        public Expr(Expression expression)
        {
            if (!typeof(T).IsAssignableFrom(expression.Type))
            {
                throw new ArgumentException($"Invalid expression type '{expression.Type.FullName}'. Expected '{typeof(T).FullName}'.", nameof(expression));
            }

            Expression = expression;
        }

        public readonly Expression Expression;

        public static implicit operator Expression(Expr<T> expr) => expr.Expression;
    }

    public struct ParamExpr<T>
    {
        public ParamExpr(ParameterExpression expression)
        {
            if (!typeof(T).IsAssignableFrom(expression.Type))
            {
                throw new ArgumentException($"Invalid expression type '{expression.Type.FullName}'. Expected '{typeof(T).FullName}'.", nameof(expression));
            }

            Expression = expression;
        }

        public readonly ParameterExpression Expression;

        public static implicit operator ParameterExpression(ParamExpr<T> expr) => expr.Expression;
        public static implicit operator Expr<T>(ParamExpr<T> expr) => expr.Expression.As<T>();
    }


    public static class Expr
    {
        public static ParamExpr<T> Parameter<T>() => new ParamExpr<T>(Expression.Parameter(typeof(T)));
        public static ParamExpr<T> Parameter<T>(string name) => new ParamExpr<T>(Expression.Parameter(typeof(T), name));

        public static ParamExpr<T> Variable<T>() => new ParamExpr<T>(Expression.Variable(typeof(T)));
        public static ParamExpr<T> Variable<T>(string name) => new ParamExpr<T>(Expression.Variable(typeof(T), name));
    }
}
#endif