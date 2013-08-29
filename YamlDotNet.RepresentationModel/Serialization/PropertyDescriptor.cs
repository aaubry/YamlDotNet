using System;

namespace YamlDotNet.RepresentationModel.Serialization
{
	public sealed class PropertyDescriptor : IPropertyDescriptor
	{
		private readonly IPropertyDescriptor baseDescriptor;

		public PropertyDescriptor(IPropertyDescriptor baseDescriptor)
		{
			this.baseDescriptor = baseDescriptor;
			Name = baseDescriptor.Name;
			Type = baseDescriptor.Type;
			StaticType = baseDescriptor.StaticType;
		}

		public string Name { get; set; }
		public Type Type { get; set; }
		public Type StaticType { get; set; }

		public object Value
		{
			get { return baseDescriptor.Value; }
		}

		public bool CanWrite
		{
			get { return baseDescriptor.CanWrite; }
		}

		public void SetValue(object target, object value)
		{
			baseDescriptor.SetValue(target, value);
		}

		public T GetCustomAttribute<T>() where T : Attribute
		{
			return baseDescriptor.GetCustomAttribute<T>();
		}
	}
}
