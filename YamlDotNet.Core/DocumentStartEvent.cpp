#include "StdAfx.h"
#include "DocumentStartEvent.h"
#include "YamlException.h"

namespace YamlDotNet {
	namespace Core {
		DocumentStartEvent::DocumentStartEvent(const yaml_event_t* nativeEvent)
			: YamlEvent(nativeEvent)
		{
			if(nativeEvent->data.document_start.version_directive != NULL) {
				version = YamlVersion(nativeEvent->data.document_start.version_directive);
			}
		}

		DocumentStartEvent::DocumentStartEvent(YamlVersion _version)
			: version(_version)
		{
		}

		DocumentStartEvent::~DocumentStartEvent()
		{
		}

		YamlVersion DocumentStartEvent::Version::get() {
			return version;
		}

		bool DocumentStartEvent::IsImplicit::get() {
			if(NativeEvent != NULL) {
				return NativeEvent->data.document_start.implicit != 0;
			} else {
				return false;
			}
		}

		String^ DocumentStartEvent::ToString() {
			return String::Format(System::Globalization::CultureInfo::InvariantCulture, 
				"{0} {1} {2}.{3}",
				GetType()->Name,
				IsImplicit ? "implicit" : "explicit",
				Version.Major,
				Version.Minor
			);
		}

		void DocumentStartEvent::CreateEvent(yaml_event_t* nativeEvent) {
			yaml_version_directive_t version;

			bool hasVersion = Version.Major != 0 || Version.Minor != 0;
			if(hasVersion) {
				version.major = Version.Major;
				version.minor = Version.Minor;
			}

			// TODO: Allow to specify directives
			int result = yaml_document_start_event_initialize(
				nativeEvent,
				hasVersion ? &version : NULL,
				NULL,
				NULL,
				IsImplicit ? 1 : 0
			);

			if(result != 1) {
				throw gcnew YamlException();
			}
		}
	}
}