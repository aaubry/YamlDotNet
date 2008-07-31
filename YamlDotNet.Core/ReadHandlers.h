#pragma once

using namespace YamlDotNet::Core::LibYaml;

namespace YamlDotNet {
	namespace Core {
		int StreamReadHandler(void *data, unsigned char* buffer, size_t size, size_t *size_read);
	}
}