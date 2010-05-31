using System;
using System.Collections.Generic;

namespace YamlDotNet.RepresentationModel.Serialization
{
	internal sealed class ObjectAnchorCollection
	{
		private readonly IDictionary<string, object> objectsByAnchor = new Dictionary<string, object>();
		private readonly IDictionary<object, string> anchorsByObject = new Dictionary<object, string>();

		/// <summary>
		/// Adds the specified anchor.
		/// </summary>
		/// <param name="anchor">The anchor.</param>
		/// <param name="object">The @object.</param>
		public void Add(string anchor, object @object)
		{
			objectsByAnchor.Add(anchor, @object);
			if (@object != null)
			{
				anchorsByObject.Add(@object, anchor);
			}
		}

		/// <summary>
		/// Gets the anchor for the specified object.
		/// </summary>
		/// <param name="object">The object.</param>
		/// <param name="anchor">The anchor.</param>
		/// <returns></returns>
		public bool TryGetAnchor(object @object, out string anchor)
		{
			return anchorsByObject.TryGetValue(@object, out anchor);
		}

		/// <summary>
		/// Gets the <see cref="System.Object"/> with the specified anchor.
		/// </summary>
		/// <value></value>
		public object this[string anchor]
		{
			get
			{
				return objectsByAnchor[anchor];
			}
		}
	}
}