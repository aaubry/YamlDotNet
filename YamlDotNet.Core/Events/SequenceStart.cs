using System;

namespace YamlDotNet.Core.Events
{
    public class SequenceStart : Event, INodeEvent
	{
		private string anchor;		
		
    	public string Anchor {
    		get {
    			return anchor;
    		}
    	}

		private string tag;
		
    	public string Tag {
    		get {
				return tag;
    		}
    	}

		public SequenceStart(string anchor, string tag, Mark start, Mark end)
			: base(start, end)
		{
			this.anchor = anchor;
			this.tag = tag;
		}
		
		public SequenceStart() {
		}
	}
}
