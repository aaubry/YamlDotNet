#include "StdAfx.h"
#include "SequenceStartEvent.h"
#include "StringConverter.h"
#include "YamlException.h"

namespace YamlDotNet {
	namespace Core {
		SequenceStartEvent::SequenceStartEvent(const yaml_event_t* nativeEvent)
			: YamlEvent(nativeEvent)
		{
			style = (ScalarStyle)nativeEvent->data.sequence_start.style;
			isImplicit = nativeEvent->data.sequence_start.implicit != 0;
		}

		SequenceStartEvent::SequenceStartEvent()
			: tag(nullptr), anchor(nullptr), style(ScalarStyle::Plain), isImplicit(true)
		{
		}

		SequenceStartEvent::SequenceStartEvent(String^ _tag)
			: tag(_tag), anchor(nullptr), style(ScalarStyle::Plain), isImplicit(true)
		{
		}

		SequenceStartEvent::SequenceStartEvent(String^ _tag, String^ _anchor)
			: tag(_tag), anchor(_anchor), style(ScalarStyle::Plain), isImplicit(true)
		{
		}

		SequenceStartEvent::SequenceStartEvent(String^ _tag, String^ _anchor, ScalarStyle _style)
			: tag(_tag), anchor(_anchor), style(_style), isImplicit(true)
		{
		}

		SequenceStartEvent::SequenceStartEvent(String^ _tag, String^ _anchor, ScalarStyle _style, bool _isImplicit)
			: tag(_tag), anchor(_anchor), style(_style), isImplicit(_isImplicit)
		{
		}

		SequenceStartEvent::~SequenceStartEvent()
		{
		}

		String^ SequenceStartEvent::Anchor::get() {
			if(anchor == nullptr && NativeEvent != NULL) {
				anchor = StringConverter::Convert(NativeEvent->data.sequence_start.anchor);
			}
			return anchor;
		}

		String^ SequenceStartEvent::Tag::get() {
			if(tag == nullptr && NativeEvent != NULL) {
				tag = StringConverter::Convert(NativeEvent->data.sequence_start.tag);
			}
			return tag;
		}

		bool SequenceStartEvent::IsImplicit::get() {
			return isImplicit;
		}

		ScalarStyle SequenceStartEvent::Style::get() {
			return style;
		}

		String^ SequenceStartEvent::ToString() {
			return String::Format(System::Globalization::CultureInfo::InvariantCulture, 
				"{0} {1} {2} {3} {4}",
				GetType()->Name,
				Anchor,
				Tag,
				IsImplicit ? "implicit" : "explicit",
				Style
			);
		}

		void SequenceStartEvent::CreateEvent(yaml_event_t* nativeEvent) {
			yaml_char_t* anchorBuffer = StringConverter::Convert(Anchor);
			yaml_char_t* tagBuffer = StringConverter::Convert(Tag);

			// TODO: Allow to specify the style?
			int result = yaml_sequence_start_event_initialize(nativeEvent, anchorBuffer, tagBuffer, IsImplicit ? 1 : 0, YAML_ANY_SEQUENCE_STYLE);

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