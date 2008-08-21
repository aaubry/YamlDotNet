#pragma once

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Text;
using namespace YamlDotNet::Core::LibYaml;

namespace YamlDotNet {
	namespace Core {
		class StringConverter
		{
		public:
			static String^ Convert(yaml_char_t* text) {
				if(text == NULL) {
					return nullptr;
				} else {
					return gcnew String((char*)text);
				}
			}

			static yaml_char_t* Convert(String^ text) {
				if(text == nullptr) {
					return NULL;
				} else {
					int length = Encoding::UTF8->GetByteCount(text);
					array<unsigned char>^ bytes = gcnew array<unsigned char>(length);
					Encoding::UTF8->GetBytes(text, 0, text->Length, bytes, 0);
					yaml_char_t* buffer = new yaml_char_t[length + 1];
					Marshal::Copy(bytes, 0, IntPtr(buffer), length);
					buffer[length] = 0;
					return buffer;
				}
			}
		};
	}
}