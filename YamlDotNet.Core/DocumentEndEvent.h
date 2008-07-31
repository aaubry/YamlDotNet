#pragma once

#include "YamlEvent.h"

using namespace System;
using namespace YamlDotNet::Core::LibYaml;

namespace YamlDotNet {
	namespace Core {
		public ref class DocumentEndEvent : public YamlEvent
		{
		internal:
			DocumentEndEvent(const yaml_event_t* nativeEvent);
			virtual void CreateEvent(yaml_event_t* nativeEvent) override;

		public:
			DocumentEndEvent();
			virtual ~DocumentEndEvent();

			property bool IsImplicit {
				bool get();
			}

			virtual String^ ToString() override;
		};
	}
}