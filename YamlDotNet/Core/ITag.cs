using System;
using System.Diagnostics.CodeAnalysis;

namespace YamlDotNet.Core
{
    public interface ITag
    {
        TagName Name { get; }
        ScalarParser? ScalarParser { get; }
    }

    public delegate object? ScalarParser(Events.Scalar scalar);

    public sealed class SimpleTag : ITag, IEquatable<SimpleTag>
    {
        public TagName Name { get; }
        public ScalarParser? ScalarParser { get; }

        public SimpleTag(TagName name, ScalarParser? scalarParser = null)
        {
            Name = name;
            ScalarParser = scalarParser;
        }

        public static readonly SimpleTag NonSpecificNonPlainScalar = new SimpleTag(TagName.NonSpecific);
        public static readonly SimpleTag NonSpecificOtherNodes = new SimpleTag(TagName.Empty);

        public override bool Equals(object? obj) => Equals(obj as SimpleTag);

        public bool Equals([AllowNull] SimpleTag other)
        {
            return other != null
                && Name.Equals(other.Name);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override string ToString()
        {
            return Name.ToString();
        }
    }
}
