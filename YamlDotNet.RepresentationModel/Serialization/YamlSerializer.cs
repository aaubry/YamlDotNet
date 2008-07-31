using System;

namespace nyaml.RepresentationModel.Serialization {
	public class YamlSerializer {
		private readonly Type serializedType;

		public YamlSerializer(Type serializedType) {
			this.serializedType = serializedType;
		}

		public void Serialize(
	}
}