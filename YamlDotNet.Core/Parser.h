#pragma once

#include "YamlEvent.h"

using namespace System;
using namespace System::IO;
using namespace YamlDotNet::Core::LibYaml;

namespace YamlDotNet {
	namespace Core {
		public ref class Parser
		{
		private:
			gcroot<Stream^>* input;

			yaml_parser_t* parser;
			YamlEvent^ current;
			bool endOfStream;

		public:
			Parser(Stream^ input);
			~Parser();
			!Parser();

			property YamlEvent^ Current {
				YamlEvent^ get();
			}

			bool MoveNext();
		};
	}
}