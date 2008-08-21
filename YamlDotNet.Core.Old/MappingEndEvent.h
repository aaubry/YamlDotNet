#pragma once

#include "YamlEvent.h"

using namespace System;
using namespace YamlDotNet::Core::LibYaml;

namespace YamlDotNet {
	namespace Core {
		public ref class MappingEndEvent : public YamlEvent
		{
		internal:
			MappingEndEvent(const yaml_event_t* nativeEvent);
			virtual void CreateEvent(yaml_event_t* nativeEvent) override;

		public:
			MappingEndEvent();
			virtual ~MappingEndEvent();

			virtual String^ ToString() override;
		};
	}
}