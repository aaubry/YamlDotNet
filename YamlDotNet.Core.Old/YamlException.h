#pragma once

using namespace System;
using namespace System::Runtime::Serialization;
using namespace YamlDotNet::Core::LibYaml;

namespace YamlDotNet {
	namespace Core {
		[Serializable]
		public ref class YamlException : public Exception
		{
		private:
			static String^ MessageFromErrorType(yaml_error_type_t type);

		internal:
			YamlException(yaml_error_type_t type);

		public:
			YamlException();
			YamlException(String^ message);
			YamlException(String^ message, Exception^ inner);

		protected:
			YamlException(SerializationInfo^ info, StreamingContext context);
		};
	}
}