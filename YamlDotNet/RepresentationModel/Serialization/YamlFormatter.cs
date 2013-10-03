using System;
using System.Globalization;

namespace YamlDotNet.RepresentationModel.Serialization
{
	internal static class YamlFormatter
	{
		private static readonly NumberFormatInfo numberFormat = new NumberFormatInfo
		{
			CurrencyDecimalSeparator = ".",
			CurrencyGroupSeparator = "_",
			CurrencyGroupSizes = new[] { 3 },
			CurrencySymbol = string.Empty,
			CurrencyDecimalDigits = 99,
			NumberDecimalSeparator = ".",
			NumberGroupSeparator = "_",
			NumberGroupSizes = new[] { 3 },
			NumberDecimalDigits = 99
		};

		public static string FormatNumber(object number)
		{
			return Convert.ToString(number, numberFormat);
		}

		public static string FormatBoolean(object boolean)
		{
			return boolean.Equals(true) ? "true" : "false";
		}

		public static string FormatDateTime(object dateTime)
		{
			return ((DateTime)dateTime).ToString("o", CultureInfo.InvariantCulture);
		}

		public static string FormatTimeSpan(object timeSpan)
		{
			return ((TimeSpan)timeSpan).ToString();
		}
	}
}