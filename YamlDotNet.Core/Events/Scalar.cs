using System;

namespace YamlDotNet.Core.Events
{
    public class Scalar : Event, INodeEvent
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

		public Scalar(string anchor, string tag, Mark start, Mark end)
			: base(start, end)
		{
			this.anchor = anchor;
			this.tag = tag;
		}
	
		public Scalar(string anchor, string tag)
			: this(anchor, tag, Mark.Empty, Mark.Empty)
		{
		}
	
		public Scalar() {
		}
	}
}
