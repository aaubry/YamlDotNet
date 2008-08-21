#include "StdAfx.h"
#include "StreamEndEvent.h"
#include "YamlException.h"

namespace YamlDotNet {
	namespace Core {
		StreamEndEvent::StreamEndEvent(const yaml_event_t* nativeEvent)
			: YamlEvent(nativeEvent)
		{
		}

		StreamEndEvent::StreamEndEvent()
		{
		}

		StreamEndEvent::~StreamEndEvent()
		{
		}

		String^ StreamEndEvent::ToString() {
			return String::Format(System::Globalization::CultureInfo::InvariantCulture, "{0}", GetType()->Name);
		}

		void StreamEndEvent::CreateEvent(yaml_event_t* nativeEvent) {
			int result = yaml_stream_end_event_initialize(nativeEvent);
			if(result != 1) {
				throw gcnew YamlException();
			}
		}
	}
}