using System;
using Xunit;
using YamlDotNet.Events;
using YamlDotNet.Schemas;
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
			Assert.Equal(null, schema.GetDefaultTag(new Scalar(null, null, "boom", ScalarStyle.Plain, true, false)));
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
			Assert.Equal(SchemaBase.StrLongTag, schema.GetDefaultTag(new Scalar("true")));
			Assert.Equal(SchemaBase.StrLongTag, schema.GetDefaultTag(new Scalar("custom", "boom")));
			Assert.Equal(FailsafeSchema.MapLongTag, schema.GetDefaultTag(new MappingStart()));
			Assert.Equal(FailsafeSchema.SeqLongTag, schema.GetDefaultTag(new SequenceStart()));

			Assert.Equal(FailsafeSchema.MapLongTag, schema.ExpandTag("!!map"));
			Assert.Equal(FailsafeSchema.SeqLongTag, schema.ExpandTag("!!seq"));
			Assert.Equal(SchemaBase.StrLongTag, schema.ExpandTag("!!str"));

			Assert.Equal("!!map", schema.ShortenTag(FailsafeSchema.MapLongTag));
			Assert.Equal("!!seq", schema.ShortenTag(FailsafeSchema.SeqLongTag));
			Assert.Equal("!!str", schema.ShortenTag(SchemaBase.StrLongTag));

			TryParse(schema, "true", SchemaBase.StrLongTag, "true");
		}

		public void TestJsonSchemaCommon(IYamlSchema schema)
		{
			Assert.Equal(SchemaBase.StrLongTag, schema.GetDefaultTag(new Scalar(null, null, "true", ScalarStyle.DoubleQuoted, false, false)));
			Assert.Equal(JsonSchema.BoolLongTag, schema.GetDefaultTag(new Scalar("true")));
			Assert.Equal(JsonSchema.NullLongTag, schema.GetDefaultTag(new Scalar("null")));
			Assert.Equal(JsonSchema.IntLongTag, schema.GetDefaultTag(new Scalar("5")));
			Assert.Equal(JsonSchema.FloatLongTag, schema.GetDefaultTag(new Scalar("5.5")));

			Assert.Equal(JsonSchema.NullLongTag, schema.ExpandTag("!!null"));
			Assert.Equal(JsonSchema.BoolLongTag, schema.ExpandTag("!!bool"));
			Assert.Equal(JsonSchema.IntLongTag, schema.ExpandTag("!!int"));
			Assert.Equal(JsonSchema.FloatLongTag, schema.ExpandTag("!!float"));

			Assert.Equal("!!null", schema.ShortenTag(JsonSchema.NullLongTag));
			Assert.Equal("!!bool", schema.ShortenTag(JsonSchema.BoolLongTag));
			Assert.Equal("!!int", schema.ShortenTag(JsonSchema.IntLongTag));
			Assert.Equal("!!float", schema.ShortenTag(JsonSchema.FloatLongTag));

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
			Assert.Equal(SchemaBase.StrLongTag, schema.GetDefaultTag(new Scalar("boom")));
			Assert.Equal(JsonSchema.BoolLongTag, schema.GetDefaultTag(new Scalar("True")));
			Assert.Equal(JsonSchema.BoolLongTag, schema.GetDefaultTag(new Scalar("TRUE")));
			Assert.Equal(JsonSchema.IntLongTag, schema.GetDefaultTag(new Scalar("0x10")));

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
			Assert.Equal(expectedLongTag, tag);
			Assert.Equal(expectedValue, value);
		}
	}
}