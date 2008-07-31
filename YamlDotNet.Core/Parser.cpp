#include "StdAfx.h"
#include "Parser.h"
#include "ReadHandlers.h"
#include "YamlException.h"

namespace YamlDotNet {
	namespace Core {
		Parser::Parser(Stream^ input)
		{
			parser = new yaml_parser_t();
			yaml_parser_initialize(parser);

			this->input = new gcroot<Stream^>(input);
			yaml_parser_set_input(parser, StreamReadHandler, this->input);
		}

		Parser::~Parser()
		{
			this->!Parser();
		}

		Parser::!Parser()
		{
			if(parser != NULL) {
				delete parser;
				parser = NULL;
			}
			if(input != NULL) {
				delete input;
				input = NULL;
			}
		}

		YamlEvent^ Parser::Current::get() {
			return current;
		}

		bool Parser::MoveNext() {
			if(endOfStream) {
				return false;
			}

			yaml_event_t* nativeEvent = new yaml_event_t();
			if(yaml_parser_parse(parser, nativeEvent) == 0) {
				delete nativeEvent;

				// TODO: Throw a better exception
				throw gcnew YamlException();
			}

			endOfStream = nativeEvent->type == YAML_STREAM_END_EVENT;
			
			current = YamlEvent::Create(nativeEvent);
			return true;
		}
	}
}