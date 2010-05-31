using System.Collections.Generic;

namespace YamlDotNet.RepresentationModel
{
	/// <summary>
	/// Holds state that is used when emitting a stream.
	/// </summary>
	internal class EmitterState
	{
		private readonly HashSet<string> emittedAnchors = new HashSet<string>();

		/// <summary>
		/// Gets the already emitted anchors.
		/// </summary>
		/// <value>The emitted anchors.</value>
		public HashSet<string> EmittedAnchors
		{
			get
			{
				return emittedAnchors;
			}
		}
	}
}