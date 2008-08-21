#pragma once

using namespace YamlDotNet::Core::LibYaml;

namespace YamlDotNet {
	namespace Core {
		public value class YamlVersion
		{
		private:
			int major;
			int minor;

		internal:
			YamlVersion(const yaml_version_directive_t* version);
		
		public:
			YamlVersion(int major, int minor);

			property int Major {
				int get();
			}

			property int Minor {
				int get();
			}
		};
	}
}