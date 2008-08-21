#include "StdAfx.h"
#include "YamlVersion.h"

namespace YamlDotNet {
	namespace Core {
		YamlVersion::YamlVersion(const yaml_version_directive_t* version)
			: major(version->major), minor(version->minor)
		{
		}

		YamlVersion::YamlVersion(int _major, int _minor)
			: major(_major), minor(_minor)
		{
		}

		int YamlVersion::Major::get() {
			return major;
		}

		int YamlVersion::Minor::get() {
			return minor;
		}
	}
}