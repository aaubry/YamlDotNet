using System;
using System.Collections.Generic;
using System.Text;
using YamlDotNet.Core;

namespace YamlDotNet.Serialization.Schemas
{
    public sealed class FailsafeSchema
    {
        public static class Tags
        {
            public static readonly TagName Map = new TagName("tag:yaml.org,2002:map");
            public static readonly TagName Seq = new TagName("tag:yaml.org,2002:seq");
            public static readonly TagName Str = new TagName("tag:yaml.org,2002:str");
        }
    }

    public sealed class JsonSchema
    {
        public static class Tags
        {
            public static readonly TagName Null = new TagName("tag:yaml.org,2002:null");
            public static readonly TagName Bool = new TagName("tag:yaml.org,2002:bool");
            public static readonly TagName Int = new TagName("tag:yaml.org,2002:int");
            public static readonly TagName Float = new TagName("tag:yaml.org,2002:float");
        }
    }

    public sealed class CoreSchema
    {
        public static class Tags
        {
        }
    }

    public sealed class DefaultSchema
    {
        public static class Tags
        {
            public static readonly TagName Timestamp = new TagName("tag:yaml.org,2002:timestamp");
        }
    }
}
