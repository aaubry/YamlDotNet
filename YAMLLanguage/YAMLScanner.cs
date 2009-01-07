using System;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Company.YAMLLanguage
{
	public class YAMLScanner : IScanner
	{
		private IVsTextLines buffer;

		public YAMLScanner(IVsTextLines buffer)
		{
			this.buffer = buffer;
		}

		private string currentSource;
		private int currentOffset;

		#region IScanner Members
		public bool ScanTokenAndProvideInfoAboutIt(TokenInfo tokenInfo, ref int state)
		{
			buffer.
		}

		public void SetSource(string source, int offset)
		{
			currentSource = source;
			currentOffset = offset;
		}
		#endregion
	}
}