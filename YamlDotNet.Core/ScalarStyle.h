#pragma once

using namespace YamlDotNet::Core::LibYaml;

namespace YamlDotNet {
	namespace Core {
		public enum class ScalarStyle
		{
			Any = YAML_ANY_SCALAR_STYLE,
			Plain = YAML_PLAIN_SCALAR_STYLE,
			SingleQuoted = YAML_SINGLE_QUOTED_SCALAR_STYLE,
			DoubleQuoted = YAML_DOUBLE_QUOTED_SCALAR_STYLE,
			Literal = YAML_LITERAL_SCALAR_STYLE,
			Folded = YAML_FOLDED_SCALAR_STYLE,
		};
	}
}