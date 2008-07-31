#include "StdAfx.h"
#include "ReadHandlers.h"

using namespace System;
using namespace System::IO;
using namespace System::Runtime::InteropServices;

namespace YamlDotNet {
	namespace Core {
		int StreamReadHandler(void *data, unsigned char* buffer, size_t size, size_t *size_read) {
			gcroot<Stream^>* input = (gcroot<Stream^>*)data;

			array<unsigned char>^ managedBuffer = gcnew array<unsigned char>(size);
			int count = (*input)->Read(managedBuffer, 0, size);
			Marshal::Copy(managedBuffer, 0, IntPtr(buffer), count);
			*size_read = count;
			return 1;
		}
	}
}