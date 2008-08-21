#pragma once

#include "YamlEvent.h"

using namespace System;
using namespace YamlDotNet::Core::LibYaml;

namespace YamlDotNet {
	namespace Core {
		public ref class StreamStartEvent : public YamlEvent
		{
		private:
			System::Text::Encoding^ encoding;

		internal:
			StreamStartEvent(const yaml_event_t* nativeEvent);
			virtual void CreateEvent(yaml_event_t* nativeEvent) override;

		public:
			StreamStartEvent(System::Text::Encoding^ encoding);
			virtual ~StreamStartEvent();

			property System::Text::Encoding^ Encoding {
				System::Text::Encoding^ get();
			}

			virtual String^ ToString() override;
		};
	}
}