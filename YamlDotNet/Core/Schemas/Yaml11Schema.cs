using System.Collections.Generic;
using YamlDotNet.Core.Events;

namespace YamlDotNet.Core.Schemas
{
    // TODO
    /// <summary>
    /// Implements the YAML 1.1 schema: <see href="https://yaml.org/spec/1.2/spec.html#id2802346"/>.
    /// </summary>
    public sealed class Yaml11Schema : RegexBasedSchema
    {
        private Yaml11Schema() : base(BuildMappingTable(), YamlTagRepository.String) { }

        public static readonly Yaml11Schema Instance = new Yaml11Schema();

        private static RegexTagMappingTable BuildMappingTable() => new RegexTagMappingTable
        {
            {
                "^(y|Y|yes|Yes|YES|n|N|no|No|NO|true|True|TRUE|false|False|FALSE|on|On|ON|off|Off|OFF)$",
                YamlTagRepository.Boolean
            },
            {
                "^[-+]?0b[0-1_]+$", // Base 2
                YamlTagRepository.Integer
            },
            {
                "^[-+]?0[0-7_]+$", // Base 8
                YamlTagRepository.Integer
            },
            {
                "^[-+]?(0|[1-9][0-9_]*)$", // Base 10
                YamlTagRepository.Integer
            },
            {
                "^[-+]?0x[0-9a-fA-F_]+$", // Base 16
                YamlTagRepository.Integer
            },
            {
                "^[-+]?[1-9][0-9_]*(:[0-5]?[0-9])+$", // Base 60
                YamlTagRepository.Integer
            },
            {
                @"^[-+]?([0-9][0-9_]*)?\.[0-9_]*([eE][-+][0-9]+)?$", // Base 10
                YamlTagRepository.FloatingPoint
            },
            {
                @"^[-+]?[0-9][0-9_]*(:[0-5]?[0-9])+\.[0-9_ ]*$", // Base 60
                YamlTagRepository.FloatingPoint
            },
            {
                @"^[-+]?\.(inf|Inf|INF)$", // Infinity
                YamlTagRepository.FloatingPoint
            },
            {
                @"^\.(nan|NaN|NAN)$", // Non a number
                YamlTagRepository.FloatingPoint
            },
            {
                "^(null|Null|NULL|~?)$", // Null
                YamlTagRepository.Null
            },
        };

    }
}
