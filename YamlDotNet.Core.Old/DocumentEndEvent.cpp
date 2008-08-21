#include "StdAfx.h"
#include "DocumentEndEvent.h"
#include "YamlException.h"

namespace YamlDotNet {
	namespace Core {
		DocumentEndEvent::DocumentEndEvent(const yaml_event_t* nativeEvent)
			: YamlEvent(nativeEvent)
		{
		}

		DocumentEndEvent::DocumentEndEvent()
		{
		}

		DocumentEndEvent::~DocumentEndEvent()
		{
		}

		bool DocumentEndEvent::IsImplicit::get() {
			if(NativeEvent != NULL) {
				return NativeEvent->data.document_end.implicit != 0;
			} else {
				return false;
			}
		}

		String^ DocumentEndEvent::ToString() {
			return String::Format(System::Globalization::CultureInfo::InvariantCulture, "{0} {1}", GetType()->Name, IsImplicit ? "implicit" : "explicit");
		}

		void DocumentEndEvent::CreateEvent(yaml_event_t* nativeEvent) {
			int result = yaml_document_end_event_initialize(nativeEvent, IsImplicit ? 1 : 0);

			if(result != 1) {
				throw gcnew YamlException();
			}
		}
	}
}