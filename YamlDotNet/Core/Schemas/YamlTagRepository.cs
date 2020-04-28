namespace YamlDotNet.Core.Schemas
{
    public static class YamlTagRepository
    {
        public const string Prefix = "tag:yaml.org,2002:";

        /// <summary>
        /// <see href="https://yaml.org/spec/1.2/spec.html#id2802432"/>
        /// <see href="https://yaml.org/type/map.html"/>
        /// </summary>
        public static readonly TagName Mapping = new TagName(Prefix + "map");

        /// <summary>
        /// <see href="https://yaml.org/spec/1.2/spec.html#id2802662"/>
        /// <see href="https://yaml.org/type/seq.html"/>
        /// </summary>
        public static readonly TagName Sequence = new TagName(Prefix + "seq");

        // TODO: Implement this later
        /// <summary>
        /// <see href="https://yaml.org/type/omap.html"/>
        /// </summary>
        public static readonly TagName OrderedMapping = new TagName(Prefix + "omap");

        // TODO: Implement this later
        /// <summary>
        /// <see href="https://yaml.org/type/pairs.html"/>
        /// </summary>
        public static readonly TagName Pairs = new TagName(Prefix + "pairs");

        // TODO: Implement this later
        /// <summary>
        /// <see href="https://yaml.org/type/set.html"/>
        /// </summary>
        public static readonly TagName Set = new TagName(Prefix + "set");



        /// <summary>
        /// <see href="https://yaml.org/spec/1.2/spec.html#id2802842"/>
        /// <see href="https://yaml.org/type/str.html"/>
        /// </summary>
        public static readonly TagName String = new TagName(Prefix + "str");

        /// <summary>
        /// <see href="https://yaml.org/spec/1.2/spec.html#id2803362"/>
        /// <see href="https://yaml.org/type/null.html"/>
        /// </summary>
        public static readonly TagName Null = new TagName(Prefix + "null");

        /// <summary>
        /// <see href="https://yaml.org/spec/1.2/spec.html#id2803629"/>
        /// <see href="https://yaml.org/type/bool.html"/>
        /// </summary>
        public static readonly TagName Boolean = new TagName(Prefix + "bool");

        /// <summary>
        /// <see href="https://yaml.org/spec/1.2/spec.html#id2803828"/>
        /// <see href="https://yaml.org/type/int.html"/>
        /// </summary>
        public static readonly TagName Integer = new TagName(Prefix + "int");

        /// <summary>
        /// <see href="https://yaml.org/spec/1.2/spec.html#id2804092"/>
        /// <see href="https://yaml.org/type/float.html"/>
        /// </summary>
        public static readonly TagName FloatingPoint = new TagName(Prefix + "float");



        // TODO: Implement this later
        /// <summary>
        /// <see href="https://yaml.org/type/binary.html"/>
        /// </summary>
        public static readonly TagName Binary = new TagName(Prefix + "binary");

        // TODO: Review how this is currently implemented
        /// <summary>
        /// <see href="https://yaml.org/type/merge.html"/>
        /// </summary>
        public static readonly TagName Merge = new TagName(Prefix + "merge");

        // TODO: Implement this later
        /// <summary>
        /// <see href="https://yaml.org/type/timestamp.html"/>
        /// </summary>
        public static readonly TagName Timestamp = new TagName(Prefix + "timestamp");

        // TODO: Implement this later
        /// <summary>
        /// <see href="https://yaml.org/type/value.html"/>
        /// </summary>
        public static readonly TagName Value = new TagName(Prefix + "value");
    }
}
