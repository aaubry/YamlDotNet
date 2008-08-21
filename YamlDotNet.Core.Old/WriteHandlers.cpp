#include "StdAfx.h"
#include "WriteHandlers.h"

using namespace System;
using namespace System::IO;
using namespace System::Runtime::InteropServices;

namespace YamlDotNet {
	namespace Core {
		int StreamWriteHandler(void *data, unsigned char* buffer, size_t size) {
			gcroot<Stream^>* output = (gcroot<Stream^>*)data;

			array<unsigned char>^ managedBuffer = gcnew array<unsigned char>(size);
			Marshal::Copy(IntPtr(buffer), managedBuffer, 0, size);
			
			(*output)->Write(managedBuffer, 0, size);

			return 1;
		}
	}
}