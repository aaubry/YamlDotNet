using System;
using System.Collections.Generic;

namespace YamlDotNet.Serialization
{
	/// <summary>
	/// Provides a descriptor for a <see cref="System.Collections.ICollection"/>.
	/// </summary>
	public class CollectionDescriptor : ObjectDescriptor
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CollectionDescriptor" /> class.
		/// </summary>
		/// <param name="settings">The settings.</param>
		/// <param name="type">The type.</param>
		public CollectionDescriptor(YamlSerializerSettings settings, Type type) : base(settings, type)
		{
		}

		protected override List<IMemberDescriptor> PrepareMembers()
		{
			var members = base.PrepareMembers();

			// In case we are not emitting List.Capacity, we need to remove them from the member list
			if (!Settings.EmitCapacityForList)
			{
				for (int i = members.Count - 1; i >= 0; i--)
				{
					if (members[i].Name == "Capacity" && members[i].Type == typeof (int))
					{
						members.RemoveAt(i);
						break;
					}
				}
			}
			return members;
		}
	}
}