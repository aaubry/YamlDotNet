using System;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio;

namespace Company.YAMLLanguage
{
	class YAMLAuthoringScope : AuthoringScope
	{
		public override string GetDataTipText(int line, int col, out TextSpan span)
		{
			span = new TextSpan();
			return null;
		}

		public override Declarations GetDeclarations(IVsTextView view, int line, int col, TokenInfo info, ParseReason reason)
		{
			return null;
		}

		public override Methods GetMethods(int line, int col, string name)
		{
			return null;
		}

		public override string Goto(VSConstants.VSStd97CmdID cmd, IVsTextView textView, int line, int col, out TextSpan span)
		{
			span = new TextSpan();
			return null;
		}
	}
}