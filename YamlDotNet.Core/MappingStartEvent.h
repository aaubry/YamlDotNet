#pragma once

#include "YamlEvent.h"
#include "ScalarStyle.h"
#include "INodeEvent.h"

using namespace System;
using namespace YamlDotNet::Core::LibYaml;

namespace YamlDotNet {
	namespace Core {
		public ref class MappingStartEvent : public YamlEvent, public INodeEvent
		{
		private:
			String^ anchor;
			String^ tag;
			ScalarStyle style;
			bool isImplicit;

		internal:
			MappingStartEvent(const yaml_event_t* nativeEvent);
			virtual void CreateEvent(yaml_event_t* nativeEvent) override;

		public:
			MappingStartEvent();
			MappingStartEvent(String^ tag);
			MappingStartEvent(String^ tag, String^ anchor);
			MappingStartEvent(String^ tag, String^ anchor, ScalarStyle style);
			MappingStartEvent(String^ tag, String^ anchor, ScalarStyle style, bool isImplicit);
			virtual ~MappingStartEvent();

			virtual property String^ Anchor {
				String^ get();
			}

			virtual property String^ Tag {
				String^ get();
			}

			property bool IsImplicit {
				bool get();
			}

			property ScalarStyle Style {
				ScalarStyle get();
			}

			virtual String^ ToString() override;
		};
	}
}