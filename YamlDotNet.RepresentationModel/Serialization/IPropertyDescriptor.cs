using System;

namespace YamlDotNet.RepresentationModel.Serialization
{
	public interface IPropertyDescriptor : IObjectDescriptor
	{
		string Name { get; }
		bool CanWrite { get; }

		void SetValue(object target, object value);

		T GetCustomAttribute<T>() where T : Attribute;
	}
}