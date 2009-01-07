using System;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Company.YAMLLanguage
{
	public class YAMLLanguageService : LanguageService
	{
		private LanguagePreferences preferences;

		public override LanguagePreferences GetLanguagePreferences()
		{
			if(preferences == null)
			{
				preferences = new LanguagePreferences(Site, typeof(YAMLLanguageService).GUID, Name);
				preferences.Init();
			}
			return preferences;
		}

		private YAMLScanner scanner;

		public override IScanner GetScanner(IVsTextLines buffer)
		{
			if(scanner == null)
			{
				scanner = new YAMLScanner(buffer);
			}
			return scanner;
		}

		public override AuthoringScope ParseSource(ParseRequest req)
		{
			return new YAMLAuthoringScope();
		}

		public override string GetFormatFilterList()
		{
			return "YAML files (*.yaml)\n*.yaml\n";
		}

		public override string Name
		{
			get
			{
				return "YAML";
			}
		}
	}
}