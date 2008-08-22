using System;
using System.Collections.Generic;
using YamlDotNet.Core.Tokens;

namespace YamlDotNet.Core.Events
{
    public class DocumentStart : Event
	{
		private readonly IList<TagDirective> tags;
		private readonly VersionDirective version;
		
		public IList<TagDirective> Tags {
			get {
				return tags;
			}
		}

		public VersionDirective Version {
			get {
				return version;
			}
		}
		
		public DocumentStart(Tokens.VersionDirective version, IList<Tokens.TagDirective> tags, Mark start, Mark end)
			: base(start, end)
		{
			this.version = version;
			this.tags = tags;
		}
		
		public DocumentStart(Tokens.VersionDirective version, IList<Tokens.TagDirective> tags)
			: this(version, tags, Mark.Empty, Mark.Empty)
		{
		}
		
		public DocumentStart(Mark start, Mark end)
			: this(null, null, start, end)
		{
		}
		
		public DocumentStart() {
		}
	}
}
