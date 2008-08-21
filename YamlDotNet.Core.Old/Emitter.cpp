#include "StdAfx.h"
#include "Emitter.h"
#include "WriteHandlers.h"
#include "YamlException.h"

namespace YamlDotNet {
	namespace Core {
		Emitter::Emitter(Stream^ output)
		{
			emitter = new yaml_emitter_t();
			yaml_emitter_initialize(emitter);

			this->output = new gcroot<Stream^>(output);
			yaml_emitter_set_output(emitter, StreamWriteHandler, this->output);
		}

		Emitter::~Emitter()
		{
			this->!Emitter();
		}

		Emitter::!Emitter()
		{
			if(emitter != NULL) {
				delete emitter;
				emitter = NULL;
			}
			if(output != NULL) {
				delete output;
				output = NULL;
			}
		}

		void Emitter::Emit(YamlEvent^ yamlEvent) {
			yaml_event_t yamlEvt;
			yamlEvent->CreateEvent(&yamlEvt);
			int result = yaml_emitter_emit(emitter, &yamlEvt);

			if(result != 1) {
				throw gcnew YamlException(emitter->error);
			}
		}
	}
}