#include "StdAfx.h"
#include "Mark.h"

namespace YamlDotNet {
	namespace Core {
		Mark::Mark(const yaml_mark_t* mark)
			: index(mark->index), line(mark->line), column(mark->column)
		{
		}

		unsigned Mark::Index::get() {
			return index;
		}

		unsigned Mark::Line::get() {
			return line;
		}

		unsigned Mark::Column::get() {
			return column;
		}
	}
}