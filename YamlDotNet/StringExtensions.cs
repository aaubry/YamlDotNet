namespace YamlDotNet
{
	/// <summary>
	/// Add extensions methods to <see cref="System.String"/>.
	/// </summary>
	internal static class StringExtension
	{
		/// <summary>
		/// Expression of string.Format(this, arg1, arg2, ...)
		/// </summary>
		/// <param name="format">The format string.</param>
		/// <param name="args">The arguments.</param>
		/// <returns>A formatted string.</returns>
		/// <see cref="string.Format(System.IFormatProvider,string,object[])"/>
		public static string DoFormat(this string format, params object[] args)
		{
			return string.Format(format, args);
		}
	}
}