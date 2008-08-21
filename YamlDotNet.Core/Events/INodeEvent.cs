
using System;

namespace YamlDotNet.CoreCs.Events
{
	public interface INodeEvent
	{
		string Anchor {
			get;
		}
		
		string Tag {
			get;
		}
	}
}
