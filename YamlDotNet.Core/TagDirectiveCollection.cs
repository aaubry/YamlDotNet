using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using YamlDotNet.Core.Tokens;

namespace YamlDotNet.Core
{
	public class TagDirectiveCollection : KeyedCollection<string, TagDirective>
	{
		public TagDirectiveCollection() {
		}
		
		public TagDirectiveCollection(IEnumerable<TagDirective> tagDirectives) {
			foreach (TagDirective tagDirective in tagDirectives) {
				Add(tagDirective);
			}
		}
		
		protected override string GetKeyForItem(TagDirective item)
		{
			return item.Handle;
		}
	}
}