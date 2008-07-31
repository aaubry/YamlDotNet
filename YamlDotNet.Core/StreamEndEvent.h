#pragma once

#include "YamlEvent.h"

using namespace System;
using namespace YamlDotNet::Core::LibYaml;

namespace YamlDotNet {
	namespace Core {
		public ref class StreamEndEvent : public YamlEvent
		{
		internal:
			StreamEndEvent(const yaml_event_t* nativeEvent);
			virtual void CreateEvent(yaml_event_t* nativeEvent) override;

		public:
			StreamEndEvent();
			virtual ~StreamEndEvent();

			virtual String^ ToString() override;
		};
	}
}