#include "StdAfx.h"
#include "MappingStartEvent.h"
#include "StringConverter.h"
#include "YamlException.h"

namespace YamlDotNet {
	namespace Core {
		MappingStartEvent::MappingStartEvent(const yaml_event_t* nativeEvent)
			: YamlEvent(nativeEvent)
		{
			style = (ScalarStyle)nativeEvent->data.mapping_start.style;
			isImplicit = nativeEvent->data.mapping_start.implicit != 0;
		}

		MappingStartEvent::MappingStartEvent()
			: tag(nullptr), anchor(nullptr), style(ScalarStyle::Plain), isImplicit(true)
		{
		}

		MappingStartEvent::MappingStartEvent(String^ _tag)
			: tag(_tag), anchor(nullptr), style(ScalarStyle::Plain), isImplicit(true)
		{
		}

		MappingStartEvent::MappingStartEvent(String^ _tag, String^ _anchor)
			: tag(_tag), anchor(_anchor), style(ScalarStyle::Plain), isImplicit(true)
		{
		}

		MappingStartEvent::MappingStartEvent(String^ _tag, String^ _anchor, ScalarStyle _style)
			: tag(_tag), anchor(_anchor), style(_style), isImplicit(true)
		{
		}

		MappingStartEvent::MappingStartEvent(String^ _tag, String^ _anchor, ScalarStyle _style, bool _isImplicit)
			: tag(_tag), anchor(_anchor), style(_style), isImplicit(_isImplicit)
		{
		}

		MappingStartEvent::~MappingStartEvent()
		{
		}

		String^ MappingStartEvent::Anchor::get() {
			if(anchor == nullptr && NativeEvent != NULL) {
				anchor = StringConverter::Convert(NativeEvent->data.mapping_start.anchor);
			}
			return anchor;
		}

		String^ MappingStartEvent::Tag::get() {
			if(tag == nullptr && NativeEvent != NULL) {
				tag = StringConverter::Convert(NativeEvent->data.mapping_start.tag);
			}
			return tag;
		}

		bool MappingStartEvent::IsImplicit::get() {
			return isImplicit;
		}

		ScalarStyle MappingStartEvent::Style::get() {
			return style;
		}

		String^ MappingStartEvent::ToString() {
			return String::Format(System::Globalization::CultureInfo::InvariantCulture, 
				"{0} {1} {2} {3} {4}",
				GetType()->Name,
				Anchor,
				Tag,
				IsImplicit ? "implicit" : "explicit",
				Style
			);
		}

		void MappingStartEvent::CreateEvent(yaml_event_t* nativeEvent) {
			yaml_char_t* anchorBuffer = StringConverter::Convert(Anchor);
			yaml_char_t* tagBuffer = StringConverter::Convert(Tag);

			// TODO: Allow to specify the style?
			int result = yaml_mapping_start_event_initialize(
				nativeEvent,
				anchorBuffer,
				tagBuffer,
				IsImplicit ? 1 : 0,				
				YAML_ANY_MAPPING_STYLE
			);

			if(tagBuffer != NULL) {
				delete[] tagBuffer;
			}
			if(anchorBuffer != NULL) {
				delete[] anchorBuffer;
			}

			if(result != 1) {
				throw gcnew YamlException();
			}
		}
	}
}