#include "StdAfx.h"
#include "MappingEndEvent.h"
#include "YamlException.h"

namespace YamlDotNet {
	namespace Core {
		MappingEndEvent::MappingEndEvent(const yaml_event_t* nativeEvent)
			: YamlEvent(nativeEvent)
		{
		}

		MappingEndEvent::MappingEndEvent()
		{
		}

		MappingEndEvent::~MappingEndEvent()
		{
		}

		String^ MappingEndEvent::ToString() {
			return String::Format(System::Globalization::CultureInfo::InvariantCulture, "{0}", GetType()->Name);
		}

		void MappingEndEvent::CreateEvent(yaml_event_t* nativeEvent) {
			int result = yaml_mapping_end_event_initialize(nativeEvent);

			if(result != 1) {
				throw gcnew YamlException();
			}
		}
	}
}