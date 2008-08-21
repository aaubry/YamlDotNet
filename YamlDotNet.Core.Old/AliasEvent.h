#pragma once

#include "YamlEvent.h"

using namespace System;
using namespace YamlDotNet::Core::LibYaml;

namespace YamlDotNet {
	namespace Core {
		/// <summary>
		/// Contains informations about a YAML alias.
		/// </summary>
		public ref class AliasEvent : public YamlEvent
		{
		private:
			String^ anchor;

		internal:
			AliasEvent(const yaml_event_t* nativeEvent);
			virtual void CreateEvent(yaml_event_t* nativeEvent) override;

		public:
			AliasEvent(String^ anchor);
			virtual ~AliasEvent();

			/// <summary>
			/// Gets the value of the anchor
			/// </summary>
			property String^ Anchor {
				String^ get();
			}

			virtual String^ ToString() override;
		};
	}
}