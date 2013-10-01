using System;
using System.Text.RegularExpressions;

namespace YamlDotNet.RepresentationModel.Serialization
{
	/// <summary>
	/// Various string extension methods
	/// </summary>
	internal static class StringExtensions
	{
		private static string ToCamelOrPascalCase(string str, Func<char, char> firstLetterTransform)
		{
			var text = Regex.Replace(str, "([_\\-])(?<char>[a-z])", match => match.Groups["char"].Value.ToUpperInvariant(), RegexOptions.IgnoreCase);
			return firstLetterTransform(text[0]) + text.Substring(1);
		}
		
		
		/// <summary>
		/// Convert the string with underscores (this_is_a_test) or hyphens (this-is-a-test) to 
		/// camel case (thisIsATest). Camel case is the same as Pascal case, except the first letter
		/// is lowercase.
		/// </summary>
		/// <param name="str">String to convert</param>
		/// <returns>Converted string</returns>
		public static string ToCamelCase(this string str)
		{
			return ToCamelOrPascalCase(str, char.ToLowerInvariant);
		}

		/// <summary>
		/// Convert the string with underscores (this_is_a_test) or hyphens (this-is-a-test) to 
		/// pascal case (ThisIsATest). Pascal case is the same as camel case, except the first letter
		/// is uppercase.
		/// </summary>
		/// <param name="str">String to convert</param>
		/// <returns>Converted string</returns>
		public static string ToPascalCase(this string str)
		{
			return ToCamelOrPascalCase(str, char.ToUpperInvariant);
		}

		/// <summary>
		/// Convert the string from camelcase (thisIsATest) to a hyphenated (this-is-a-test) or 
		/// underscored (this_is_a_test) string
		/// </summary>
		/// <param name="str">String to convert</param>
		/// <param name="separator">Separator to use between segments</param>
		/// <returns>Converted string</returns>
		public static string FromCamelCase(this string str, string separator)
		{
			// Ensure first letter is always lowercase
			str = char.ToLower(str[0]) + str.Substring(1);

			str = Regex.Replace(str.ToCamelCase(), "(?<char>[A-Z])", match => separator + match.Groups["char"].Value.ToLowerInvariant());
			return str;
		}
	}
}
