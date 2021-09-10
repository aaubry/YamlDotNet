using System;
using System.Reflection;

namespace YamlDotNet.Sandbox
{
    public static class SchemaBuilderExtensions
    {
        public static ISchemaNode BuildSchema(this ISchemaBuilder schemaBuilder, Type valueType)
        {
            return (ISchemaNode)buildSchemaHelperMethod
                .MakeGenericMethod(valueType)
                .Invoke(null, new[] { schemaBuilder })!;
        }

        private static readonly MethodInfo buildSchemaHelperMethod = typeof(SchemaBuilderExtensions).GetMethod(nameof(BuildSchemaHelper), BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException($"Method '{nameof(BuildSchemaHelper)}' not found in class '{nameof(SchemaBuilderExtensions)}'.");

        private static ISchemaNode BuildSchemaHelper<TValue>(ISchemaBuilder schemaBuilder)
        {
            return schemaBuilder.BuildSchema<TValue>();
        }
    }
}
