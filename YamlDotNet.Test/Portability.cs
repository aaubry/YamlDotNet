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
                throw new ArgumentException("Expected a lambda expression in the form: x => x.SomeProperty", "propertyAccessor");
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
