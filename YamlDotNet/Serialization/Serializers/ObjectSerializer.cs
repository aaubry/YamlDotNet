using YamlDotNet.Events;

namespace YamlDotNet.Serialization.Serializers
{
    /// <summary>
	/// Default processor for serializing an object.
	/// </summary>
	public class ObjectSerializer : ObjectSerializerBase
	{
        /// <summary>
		/// Initializes a new instance of the <see cref="ObjectSerializer"/> class.
		/// </summary>
		/// <param name="settings">The settings.</param>
		/// <exception cref="System.ArgumentNullException">settings</exception>
		public ObjectSerializer(SerializerSettings settings) : base(settings)
        {
        }

        protected override bool CheckIsSequence(ITypeDescriptor typeDescriptor)
        {
            // By default an object serializer is a mapping
            return false;
        }

        protected override void ReadItem(SerializerContext context, object thisObject, ITypeDescriptor typeDescriptor)
		{
			var reader = context.Reader;

			// For a regular object, the key is expected to be a simple scalar
			var propertyName = reader.Expect<Scalar>().Value;
			var memberAccessor = typeDescriptor[propertyName];

			// Read the value according to the type
			var propertyType = memberAccessor.Type;

			object value = null;
			if (memberAccessor.SerializeMemberMode == SerializeMemberMode.Content)
			{
				value = memberAccessor.Get(thisObject);
			}

			var propertyValue = context.ReadYaml(value, propertyType);

			if (memberAccessor.HasSet && memberAccessor.SerializeMemberMode != SerializeMemberMode.Content)
			{
				memberAccessor.Set(thisObject, propertyValue);
			}
		}

        public override void WriteItems(SerializerContext context, object thisObject, ITypeDescriptor typeDescriptor)
		{
			foreach (var member in typeDescriptor.Members)
			{
				// Skip any member that we won't serialize
				if (!member.ShouldSerialize(thisObject)) continue;

				// Emit the key name
				context.Writer.Emit(new ScalarEventInfo(member.Name, typeof (string)));

				var memberValue = member.Get(thisObject);
				var memberType = member.Type;
				context.WriteYaml(memberValue, memberType);
			}
		}
	}
}