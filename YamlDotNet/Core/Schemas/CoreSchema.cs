namespace YamlDotNet.Core.Schemas
{
    /// <summary>
    /// Implements the Core schema: <see href="https://yaml.org/spec/1.2/spec.html#id2804923"/>.
    /// </summary>
    public sealed class CoreSchema : RegexBasedSchema
    {
        private CoreSchema() : base(BuildMappingTable(), YamlTagRepository.String) { }

        public static readonly CoreSchema Instance = new CoreSchema();

        private static RegexTagMappingTable BuildMappingTable() => new RegexTagMappingTable
        {
            {
                "^(null|Null|NULL|~?)$",
                YamlTagRepository.Null
            },
            {
                "^(true|True|TRUE|false|False|FALSE)$",
                YamlTagRepository.Boolean
            },
            {
                "^[-+]?[0-9]+$", // Base 10
                YamlTagRepository.Integer
            },
            {
                "^0o[0-7]+$", // Base 8
                YamlTagRepository.Integer
            },
            {
                "^0x[0-9a-fA-F]+$", // Base 16
                YamlTagRepository.Integer
            },
            {
                @"^[-+]?(\.[0-9]+|[0-9]+(\.[0-9]*)?)([eE][-+]?[0-9]+)?$",
                YamlTagRepository.FloatingPoint
            },
            {
                @"^[-+]?(\.inf|\.Inf|\.INF)$",
                YamlTagRepository.FloatingPoint
            },
            {
                @"^(\.nan|\.NaN|\.NAN)$",
                YamlTagRepository.FloatingPoint
            },
        };
    }
}
