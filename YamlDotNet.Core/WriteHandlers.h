#pragma once

using namespace YamlDotNet::Core::LibYaml;

namespace YamlDotNet {
	namespace Core {
		int StreamWriteHandler(void *data, unsigned char* buffer, size_t size);
	}
}