#pragma once

#include "YamlEvent.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace System::IO;
using namespace YamlDotNet::Core::LibYaml;

namespace YamlDotNet {
	namespace Core {
		/// <summary>
		/// 
		/// </summary>
		public ref class Emitter
		{
		private:
			yaml_emitter_t* emitter;
			gcroot<Stream^>* output;

		public:
			Emitter(Stream^ output);
			~Emitter();
			!Emitter();

			void Emit(YamlEvent^ yamlEvent);
		};
	}
}