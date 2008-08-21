#include "StdAfx.h"
#include "SequenceEndEvent.h"
#include "YamlException.h"

namespace YamlDotNet {
	namespace Core {
		SequenceEndEvent::SequenceEndEvent(const yaml_event_t* nativeEvent)
			: YamlEvent(nativeEvent)
		{
		}

		SequenceEndEvent::SequenceEndEvent()
		{
		}

		SequenceEndEvent::~SequenceEndEvent()
		{
		}

		String^ SequenceEndEvent::ToString() {
			return String::Format(System::Globalization::CultureInfo::InvariantCulture, "{0}", GetType()->Name);
		}

		void SequenceEndEvent::CreateEvent(yaml_event_t* nativeEvent) {
			int result = yaml_sequence_end_event_initialize(nativeEvent);
			if(result != 1) {
				throw gcnew YamlException();
			}
		}
	}
}