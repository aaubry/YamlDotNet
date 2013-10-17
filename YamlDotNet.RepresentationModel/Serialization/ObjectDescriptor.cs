using System;

namespace YamlDotNet.RepresentationModel.Serialization
{
	public sealed class ObjectDescriptor : IObjectDescriptor
	{
		public object Value { get; private set; }
		public Type Type { get; private set; }
		public Type StaticType { get; private set; }

		public ObjectDescriptor(object value, Type type, Type staticType)
		{
			Value = value;

			if (type == null)
			{
				throw new ArgumentNullException("type");
			}

			Type = type;

			if (staticType == null)
			{
				throw new ArgumentNullException("staticType");
			}

			StaticType = staticType;
		}
	}
}