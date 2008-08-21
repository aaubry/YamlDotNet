#pragma once

using namespace YamlDotNet::Core::LibYaml;

namespace YamlDotNet {
	namespace Core {
		public value class Mark
		{
		private:
			const unsigned index;
			const unsigned line;
			const unsigned column;

		internal:
			Mark(const yaml_mark_t* mark);

		public:
			property unsigned Index {
				unsigned get();
			}

			property unsigned Line {
				unsigned get();
			}

			property unsigned Column {
				unsigned get();
			}
		};
	}
}