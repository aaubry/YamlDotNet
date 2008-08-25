using System;

namespace YamlDotNet.Core.Events
{
	public enum MappingStyle
	{
		/// <summary>
		/// Let the emitter choose the style.
		/// </summary>
		Any,

		/// <summary>
		/// The block mapping style. 
		/// </summary>
		Block,
		
		/// <summary>
		/// The flow mapping style. 
		/// </summary>
		Flow
	}
}