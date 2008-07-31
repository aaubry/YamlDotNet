#pragma once

using namespace System;
using namespace YamlDotNet::Core::LibYaml;

namespace YamlDotNet {
	namespace Core {
		public interface class INodeEvent
		{
			property String^ Anchor {
				String^ get();
			}

			property String^ Tag {
				String^ get();
			}
		};
	}
}