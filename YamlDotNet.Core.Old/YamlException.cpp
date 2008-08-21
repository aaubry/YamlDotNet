#include "StdAfx.h"
#include "YamlException.h"

namespace YamlDotNet {
	namespace Core {
		YamlException::YamlException()
		{
		}

		YamlException::YamlException(String^ message)
			: Exception(message)
		{
		}

		YamlException::YamlException(String^ message, Exception^ inner)
			: Exception(message, inner)
		{
		}

		YamlException::YamlException(SerializationInfo^ info, StreamingContext context)
			: Exception(info, context)
		{
		}

		YamlException::YamlException(yaml_error_type_t type)
			: Exception(MessageFromErrorType(type))
		{
		}

		String^ YamlException::MessageFromErrorType(yaml_error_type_t type)
		{
			switch(type) {
				case YAML_NO_ERROR:
					return "No error is produced.";

				case YAML_MEMORY_ERROR:
					return "Cannot allocate or reallocate a block of memory.";

				case YAML_READER_ERROR:
					return "Cannot read or decode the input stream.";
				
				case YAML_SCANNER_ERROR:
					return "Cannot scan the input stream.";
				
				case YAML_PARSER_ERROR:
					return "Cannot parse the input stream.";
				
				case YAML_COMPOSER_ERROR:
					return "Cannot compose a YAML document.";

				case YAML_WRITER_ERROR:
					return "Cannot write to the output stream.";
				
				case YAML_EMITTER_ERROR:
					return "Cannot emit a YAML stream.";

				default:
					return "Unknown error";
			}
		}
	}
}