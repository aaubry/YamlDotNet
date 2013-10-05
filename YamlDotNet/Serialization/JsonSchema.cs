using System;

namespace YamlDotNet.Serialization
{
	/// <summary>
	/// Implements a JSON schema. <see cref="http://www.yaml.org/spec/1.2/spec.html#id2803231" />
	/// </summary>
	/// <remarks>
	/// The JSON schema is the lowest common denominator of most modern computer languages, and allows parsing JSON files. 
	/// A YAML processor should therefore support this schema, at least as an option. It is also strongly recommended that other schemas should be based on it. .
	/// </remarks>>
	public class JsonSchema : FailsafeSchema
	{
		private const string NullShortTag = "!!null";
		private const string NullLongTag = "tag:yaml.org,2002:null";

		/// <summary>
		/// Initializes a new instance of the <see cref="JsonSchema"/> class.
		/// </summary>
		public JsonSchema()
		{
			RegisterTag(NullShortTag, NullLongTag);
		}

		protected override void PrepareScalarRules()
		{
			// 10.2.1.1. Null
			AddScalarRule<object>("!!null", @"null", m => null, null);

			// 10.2.1.2. Boolean
			AddScalarRule<bool>("!!bool", @"true", m => true, null);
			AddScalarRule<bool>("!!bool", @"false", m => false, null);

			// 10.2.1.3. Integer
			AddScalarRule<int>("!!int", @"((0|-?[1-9][0-9_]*))", m => Convert.ToInt32(m.Value.Replace("_", "")), null);

			// 10.2.1.4. Floating Point
			AddScalarRule<double>("!!float", @"-?(0|[1-9][0-9]*)(\.[0-9]*)?([eE][-+]?[0-9]+)?", m => Convert.ToDouble(m.Value.Replace("_", "")), null);
			AddScalarRule<double>("!!float", @"\.inf", m => double.PositiveInfinity, null);
			AddScalarRule<double>("!!float", @"-\.inf", m => double.NegativeInfinity, null);
			AddScalarRule<double>("!!float", @"\.nan", m => double.NaN, null);
			
			base.PrepareScalarRules();
		}
	}
}