#if !NET20

using System;
using System.Reflection;
using System.Linq.Expressions;

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
    }
}
#endif