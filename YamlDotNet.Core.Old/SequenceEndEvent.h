#pragma once

#include "YamlEvent.h"

using namespace System;
using namespace YamlDotNet::Core::LibYaml;

namespace YamlDotNet {
	namespace Core {
		public ref class SequenceEndEvent : public YamlEvent
		{
		internal:
			SequenceEndEvent(const yaml_event_t* nativeEvent);
			virtual void CreateEvent(yaml_event_t* nativeEvent) override;

		public:
			SequenceEndEvent();
			virtual ~SequenceEndEvent();

			virtual String^ ToString() override;
		};
	}
}