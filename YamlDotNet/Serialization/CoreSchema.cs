using System;

namespace YamlDotNet.Serialization
{
	/// <summary>
	/// Implements the Core schema. <see cref="http://www.yaml.org/spec/1.2/spec.html#id2804356" />
	/// </summary>
	/// <remarks>
	/// The Core schema is an extension of the JSON schema, allowing for more human-readable presentation of the same types. 
	/// This is the recommended default schema that YAML processor should use unless instructed otherwise. 
	/// It is also strongly recommended that other schemas should be based on it. 
	/// </remarks>
	public class CoreSchema : JsonSchema
	{
		protected override void PrepareScalarRules()
		{
			AddScalarRule<bool>("!!bool", @"true|True|TRUE", m => true, null);
			AddScalarRule<bool>("!!bool", @"false|False|FALSE", m => false, null);

			AddScalarRule<int>("!!int", @"([-+]?(0|[1-9][0-9_]*))", m => Convert.ToInt32(m.Value.Replace("_", "")), null);

			// Make float before 0x/0o to improve parsing as float are more common than 0x and 0o
			AddScalarRule<double>("!!float", @"[-+]?(\.[0-9]+|[0-9]+(\.[0-9]*)?)([eE][-+]?[0-9]+)?", m => Convert.ToDouble(m.Value.Replace("_", "")), null);
	
			AddScalarRule<int>("!!int", @"0x([0-9a-fA-F_]+)", m => Convert.ToInt32(m.Groups[1].Value.Replace("_", ""), 16), null);
			AddScalarRule<int>("!!int", @"0o([0-7_]+)", m => Convert.ToInt32(m.Groups[1].Value.Replace("_", ""), 8), null);

			AddScalarRule<double>("!!float", @"\+?(\.inf|\.Inf|\.INF)", m => double.PositiveInfinity, null);
			AddScalarRule<double>("!!float", @"-(\.inf|\.Inf|\.INF)", m => double.NegativeInfinity, null);
			AddScalarRule<double>("!!float", @"\.nan|\.NaN|\.NAN", m => double.NaN, null);

			base.PrepareScalarRules();
		}
	}
}