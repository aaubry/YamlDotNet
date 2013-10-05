using YamlDotNet.Events;

namespace YamlDotNet.Serialization
{
	/// <summary>
	/// Implements the YAML failsafe schema.
	/// <see cref="http://www.yaml.org/spec/1.2/spec.html#id2802346" />
	/// </summary>
	/// <remarks>The failsafe schema is guaranteed to work with any YAML document.
	/// It is therefore the recommended schema for generic YAML tools.
	/// A YAML processor should therefore support this schema, at least as an option.</remarks>
	public class FailsafeSchema : SchemaBase
	{
		private const string MapShortTag = "!!map";
		private const string MapLongTag = "tag:yaml.org,2002:map";

		private const string SeqShortTag = "!!seq";
		private const string SeqLongTag = "tag:yaml.org,2002:seq";

		private const string StrShortTag = "!!str";
		private const string StrLongTag = "tag:yaml.org,2002:str";

		/// <summary>
		/// Initializes a new instance of the <see cref="FailsafeSchema"/> class.
		/// </summary>
		public FailsafeSchema()
		{
			RegisterTag(MapShortTag, MapLongTag);
			RegisterTag(SeqShortTag, SeqLongTag);
			RegisterTag(StrShortTag, StrLongTag);
		}

		protected override string GetDefaultTag(MappingStart nodeEvent)
		{
			return MapLongTag;
		}

		protected override string GetDefaultTag(SequenceStart nodeEvent)
		{
			return SeqLongTag;
		}

		public override bool DecodeScalar(Scalar scalar, bool parseValue, out string defaultTag, out object value)
		{
			if (base.DecodeScalar(scalar, parseValue, out defaultTag, out value))
			{
				return true;
			}

			value = parseValue ? scalar.Value : null;
			defaultTag = StrLongTag;
			return true;
		}
	}
}