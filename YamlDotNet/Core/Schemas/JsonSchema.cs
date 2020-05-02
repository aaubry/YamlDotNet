namespace YamlDotNet.Core.Schemas
{
    /// <summary>
    /// Implements the JSON schema: <see href="https://yaml.org/spec/1.2/spec.html#id2803231"/>.
    /// </summary>
    public sealed class JsonSchema : RegexBasedSchema
    {
        private JsonSchema(ITag fallbackTag) : base(BuildMappingTable(), fallbackTag) { }

        /// <summary>
        /// A version of the <see cref="JsonSchema"/> that conforms strictly to the specification
        /// by not resolving any unrecognized scalars.
        /// </summary>
        public static readonly JsonSchema Strict = new JsonSchema(SimpleTag.NonSpecificOtherNodes);

        /// <summary>
        /// A version of the <see cref="JsonSchema"/> that treats unrecognized scalars as strings.
        /// </summary>
        public static readonly JsonSchema Lenient = new JsonSchema(FailsafeSchema.String);

        private static RegexTagMappingTable BuildMappingTable() => new RegexTagMappingTable
        {
            //{
            //    "^null$",
            //    YamlTagRepository.Null
            //},
            //{
            //    "^(true|false)$",
            //    YamlTagRepository.Boolean.Yaml12
            //},
            //{
            //    "^-?(0|[1-9][0-9]*)$",
            //    YamlTagRepository.Integer
            //},
            //{
            //    @"^-?(0|[1-9][0-9]*)(\.[0-9]*)?([eE][-+]?[0-9]+)?$",
            //    YamlTagRepository.FloatingPoint
            //}
        };
    }
}
