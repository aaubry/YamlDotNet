#include "StdAfx.h"
#include "YamlEvent.h"
#include "AliasEvent.h"
#include "DocumentEndEvent.h"
#include "DocumentStartEvent.h"
#include "MappingEndEvent.h"
#include "MappingStartEvent.h"
#include "ScalarEvent.h"
#include "SequenceEndEvent.h"
#include "SequenceStartEvent.h"
#include "StreamEndEvent.h"
#include "StreamStartEvent.h"

namespace YamlDotNet {
	namespace Core {
		YamlEvent::YamlEvent(const yaml_event_t* nativeEvent)
		{
			assert(nativeEvent != NULL);
			this->nativeEvent = nativeEvent;
		}

		YamlEvent::YamlEvent()
		{
			nativeEvent = NULL;
		}

		YamlEvent::~YamlEvent()
		{
			this->!YamlEvent();
		}

		YamlEvent::!YamlEvent()
		{
			if(nativeEvent != NULL) {
				delete nativeEvent;
				nativeEvent = NULL;
			}
		}

		const yaml_event_t* YamlEvent::NativeEvent::get()
		{
			return nativeEvent;
		}

		YamlEvent^ YamlEvent::Create(const yaml_event_t* nativeEvent)
		{
			switch(nativeEvent->type) {
				case YAML_NO_EVENT:
					throw gcnew ArgumentException("Invalid event");

				case YAML_STREAM_START_EVENT:
					return gcnew StreamStartEvent(nativeEvent);

				case YAML_STREAM_END_EVENT:
					return gcnew StreamEndEvent(nativeEvent);

				case YAML_DOCUMENT_START_EVENT:
					return gcnew DocumentStartEvent(nativeEvent);

				case YAML_DOCUMENT_END_EVENT:
					return gcnew DocumentEndEvent(nativeEvent);

				case YAML_ALIAS_EVENT:
					return gcnew AliasEvent(nativeEvent);

				case YAML_SCALAR_EVENT:
					return gcnew ScalarEvent(nativeEvent);

				case YAML_SEQUENCE_START_EVENT:
					return gcnew SequenceStartEvent(nativeEvent);

				case YAML_SEQUENCE_END_EVENT:
					return gcnew SequenceEndEvent(nativeEvent);

				case YAML_MAPPING_START_EVENT:
					return gcnew MappingStartEvent(nativeEvent);

				case YAML_MAPPING_END_EVENT:
					return gcnew MappingEndEvent(nativeEvent);

				default:
					throw gcnew NotSupportedException();
			}
		}

		Mark YamlEvent::Start::get() {
			if(nativeEvent != NULL) {
				return Mark(&nativeEvent->start_mark);
			} else {
				return Mark();
			}
		}

		Mark YamlEvent::End::get() {
			if(nativeEvent != NULL) {
				return Mark(&nativeEvent->end_mark);
			} else {
				return Mark();
			}
		}
	}
}