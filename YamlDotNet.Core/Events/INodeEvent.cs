
using System;

namespace YamlDotNet.Core.Events
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
