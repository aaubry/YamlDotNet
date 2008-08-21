#pragma once

#include "Mark.h"

using namespace System;
using namespace YamlDotNet::Core::LibYaml;

namespace YamlDotNet {
	namespace Core {
		/// <summary>
		/// Contains informations about part of a YAML stream.
		/// </summary>
		public ref class YamlEvent abstract
		{
		private:
			const yaml_event_t* nativeEvent;

		protected:
			property const yaml_event_t* NativeEvent {
				const yaml_event_t* get();
			}

			YamlEvent(const yaml_event_t* nativeEvent);
			YamlEvent();

		internal:
			static YamlEvent^ Create(const yaml_event_t* nativeEvent);
			virtual void CreateEvent(yaml_event_t* nativeEvent) abstract;

		public:
			virtual ~YamlEvent();
			!YamlEvent();

			property Mark Start {
				Mark get();
			}

			property Mark End {
				Mark get();
			}
		};
	}
}