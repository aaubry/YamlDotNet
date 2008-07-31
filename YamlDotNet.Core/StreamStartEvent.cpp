#include "StdAfx.h"
#include "StreamStartEvent.h"
#include "YamlException.h"

namespace YamlDotNet {
	namespace Core {
		StreamStartEvent::StreamStartEvent(const yaml_event_t* nativeEvent)
			: YamlEvent(nativeEvent)
		{
			switch(nativeEvent->data.stream_start.encoding) {
				case YAML_ANY_ENCODING:
					encoding = nullptr;
					break;

				case YAML_UTF8_ENCODING:
					encoding = System::Text::Encoding::UTF8;
					break;

				case YAML_UTF16LE_ENCODING:
					encoding = System::Text::Encoding::Unicode;
					break;

				case YAML_UTF16BE_ENCODING:
					encoding = System::Text::Encoding::BigEndianUnicode;
					break;

				default:
					throw gcnew InvalidOperationException();			
			}
		}

		StreamStartEvent::StreamStartEvent(System::Text::Encoding^ _encoding)
			: encoding(_encoding)
		{
		}

		StreamStartEvent::~StreamStartEvent()
		{
		}

		System::Text::Encoding^ StreamStartEvent::Encoding::get() {
			return encoding;
		}

		String^ StreamStartEvent::ToString() {
			return String::Format(System::Globalization::CultureInfo::InvariantCulture, "{0} {1}", GetType()->Name, Encoding->WebName);
		}

		void StreamStartEvent::CreateEvent(yaml_event_t* nativeEvent) {
			// TODO: Allow to specify the encoding
			int result = yaml_stream_start_event_initialize(nativeEvent, YAML_UTF8_ENCODING);
			if(result != 1) {
				throw gcnew YamlException();
			}
		}
	}
}