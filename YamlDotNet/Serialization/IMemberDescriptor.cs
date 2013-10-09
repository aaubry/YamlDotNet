using System;

namespace YamlDotNet.Serialization
{
	/// <summary>
	/// Describe a member of an object.
	/// </summary>
	public interface IMemberDescriptor
	{
		/// <summary>
		/// Gets the name.
		/// </summary>
		/// <value>The name.</value>
		string Name { get; }

		/// <summary>
		/// Gets the type.
		/// </summary>
		/// <value>The type.</value>
		Type Type { get; }

		/// <summary>
		/// Gets the order of this member. 
		/// Default is -1, meaning that it is using the alphabetical order 
		/// based on the name of this property.
		/// </summary>
		/// <value>The order.</value>
		int Order { get; }

		/// <summary>
		/// Gets the mode of serialization for this member.
		/// </summary>
		/// <value>The mode.</value>
		SerializeMemberMode SerializeMemberMode { get; }

		/// <summary>
		/// Gets the value of this memeber for the specified instance.
		/// </summary>
		/// <param name="thisObject">The this object to get the value from.</param>
		/// <returns>Value of the member.</returns>
		object Get(object thisObject);

		/// <summary>
		/// Sets a value of this memeber for the specified instance.
		/// </summary>
		/// <param name="thisObject">The this object.</param>
		/// <param name="value">The value.</param>
		void Set(object thisObject, object value);

		/// <summary>
		/// Gets a value indicating whether this instance has set method.
		/// </summary>
		/// <value><c>true</c> if this instance has set method; otherwise, <c>false</c>.</value>
		bool HasSet { get; }

		/// <summary>
		/// Gets a value indicating whether this member should be serialized.
		/// </summary>
		/// <value><c>true</c> if [should serialize]; otherwise, <c>false</c>.</value>
		Func<object, bool> ShouldSerialize { get; }
	}
}