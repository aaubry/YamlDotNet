using System;
using Xunit;
using YamlDotNet.Events;
using YamlDotNet.Serialization;

namespace YamlDotNet.Test
{
	public class SchemaTests
	{
		[Fact]
		public void TestFailsafeSchema()
		{
			var schema = new FailsafeSchema();
			TestFailsafeSchemaCommon(schema);
		}

		[Fact]
		public void TestJsonSchema()
		{
			var schema = new JsonSchema();
			TestJsonSchemaCommon(schema);

			// Json should not accept plain literal
			Assert.Equal(schema.GetDefaultTag(new Scalar(null, null, "boom", ScalarStyle.Plain, true, false)), null);
		}

		[Fact]
		public void TestCoreSchema()
		{
			var schema = new CoreSchema();

			TestCoreSchemaCommon(schema);
		}

		[Fact]
		public void TestExtendedSchema()
		{
			var schema = new ExtendedSchema();

			TestCoreSchemaCommon(schema);

			TryParse(schema, "2002-12-14", ExtendedSchema.TimestampLongTag, new DateTime(2002, 12, 14));
			TryParse(schema, "2002-12-14 21:59:43.234", ExtendedSchema.TimestampLongTag, new DateTime(2002, 12, 14, 21, 59, 43, 234));
		}

		public void TestFailsafeSchemaCommon(IYamlSchema schema)
		{
			Assert.Equal(schema.GetDefaultTag(new Scalar("true")), SchemaBase.StrLongTag);
			Assert.Equal(schema.GetDefaultTag(new Scalar("custom", "boom")), SchemaBase.StrLongTag);
			Assert.Equal(schema.GetDefaultTag(new MappingStart()), FailsafeSchema.MapLongTag);
			Assert.Equal(schema.GetDefaultTag(new SequenceStart()), FailsafeSchema.SeqLongTag);

			Assert.Equal(schema.ExpandTag("!!map"), FailsafeSchema.MapLongTag);
			Assert.Equal(schema.ExpandTag("!!seq"), FailsafeSchema.SeqLongTag);
			Assert.Equal(schema.ExpandTag("!!str"), SchemaBase.StrLongTag);

			Assert.Equal(schema.ShortenTag(FailsafeSchema.MapLongTag), "!!map");
			Assert.Equal(schema.ShortenTag(FailsafeSchema.SeqLongTag), "!!seq");
			Assert.Equal(schema.ShortenTag(SchemaBase.StrLongTag), "!!str");

			TryParse(schema, "true", SchemaBase.StrLongTag, "true");
		}

		public void TestJsonSchemaCommon(IYamlSchema schema)
		{
			Assert.Equal(schema.GetDefaultTag(new Scalar(null, null, "true", ScalarStyle.DoubleQuoted, false, false)), SchemaBase.StrLongTag);
			Assert.Equal(schema.GetDefaultTag(new Scalar("true")), JsonSchema.BoolLongTag);
			Assert.Equal(schema.GetDefaultTag(new Scalar("null")), JsonSchema.NullLongTag);
			Assert.Equal(schema.GetDefaultTag(new Scalar("5")), JsonSchema.IntLongTag);
			Assert.Equal(schema.GetDefaultTag(new Scalar("5.5")), JsonSchema.FloatLongTag);

			Assert.Equal(schema.ExpandTag("!!null"), JsonSchema.NullLongTag);
			Assert.Equal(schema.ExpandTag("!!bool"), JsonSchema.BoolLongTag);
			Assert.Equal(schema.ExpandTag("!!int"), JsonSchema.IntLongTag);
			Assert.Equal(schema.ExpandTag("!!float"), JsonSchema.FloatLongTag);

			Assert.Equal(schema.ShortenTag(JsonSchema.NullLongTag), "!!null");
			Assert.Equal(schema.ShortenTag(JsonSchema.BoolLongTag), "!!bool");
			Assert.Equal(schema.ShortenTag(JsonSchema.IntLongTag), "!!int");
			Assert.Equal(schema.ShortenTag(JsonSchema.FloatLongTag), "!!float");

			TryParse(schema, "null", JsonSchema.NullLongTag, null);
			TryParse(schema, "true", JsonSchema.BoolLongTag, true);
			TryParse(schema, "false", JsonSchema.BoolLongTag, false);
			TryParse(schema, "5", JsonSchema.IntLongTag, 5);
			TryParse(schema, "5.5", JsonSchema.FloatLongTag, 5.5);
			TryParse(schema, ".inf", JsonSchema.FloatLongTag, double.PositiveInfinity);
		}

		public void TestCoreSchemaCommon(IYamlSchema schema)
		{
			TestJsonSchemaCommon(schema);

			// Core schema is accepting plain string
			Assert.Equal(schema.GetDefaultTag(new Scalar("boom")), SchemaBase.StrLongTag);
			Assert.Equal(schema.GetDefaultTag(new Scalar("True")), JsonSchema.BoolLongTag);
			Assert.Equal(schema.GetDefaultTag(new Scalar("TRUE")), JsonSchema.BoolLongTag);
			Assert.Equal(schema.GetDefaultTag(new Scalar("0x10")), JsonSchema.IntLongTag);

			TryParse(schema, "TRUE", JsonSchema.BoolLongTag, true);
			TryParse(schema, "FALSE", JsonSchema.BoolLongTag, false);
			TryParse(schema, "0x10", JsonSchema.IntLongTag, 16);
			TryParse(schema, "16", JsonSchema.IntLongTag, 16);
		}

		private void TryParse(IYamlSchema schema, string scalar, string expectedLongTag, object expectedValue)
		{
			string tag;
			object value;
			Assert.True(schema.TryParse(new Scalar(scalar), true, out tag, out value));
			Assert.Equal(tag, expectedLongTag);
			Assert.Equal(value, expectedValue);
		}
	}
}