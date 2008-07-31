#include "StdAfx.h"
#include "AliasEvent.h"
#include "StringConverter.h"
#include "YamlException.h"

using namespace System::Runtime::InteropServices;

namespace YamlDotNet {
	namespace Core {
		AliasEvent::AliasEvent(const yaml_event_t* nativeEvent)
			: YamlEvent(nativeEvent)
		{
			anchor = nullptr;
		}

		AliasEvent::AliasEvent(String^ _anchor)
			: anchor(_anchor)
		{
		}

		AliasEvent::~AliasEvent()
		{
		}

		String^ AliasEvent::Anchor::get() {
			if(anchor == nullptr && NativeEvent != NULL) {
				anchor = StringConverter::Convert(NativeEvent->data.alias.anchor);
			}
			return anchor;
		}

		String^ AliasEvent::ToString() {
			return String::Format(System::Globalization::CultureInfo::InvariantCulture, "{0} {1}", GetType()->Name, Anchor);
		}

		void AliasEvent::CreateEvent(yaml_event_t* nativeEvent) {
			yaml_char_t* anchorBuffer = StringConverter::Convert(Anchor);

			int result = yaml_alias_event_initialize(nativeEvent, anchorBuffer);

			if(anchorBuffer != NULL) {
				delete[] anchorBuffer;
			}

			if(result != 1) {
				throw gcnew YamlException();
			}
		}
	}
}