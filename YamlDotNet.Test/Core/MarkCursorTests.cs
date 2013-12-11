using FluentAssertions;
using Xunit;
using YamlDotNet.Core;

namespace YamlDotNet.Test.Core
{
	class MarkCursorTests
	{
		[Fact]
		public void ShouldProvideAnOneIndexedMark()
		{
			var cursor = new Cursor();

			var result = cursor.Mark();

			result.Line.Should().Be(1, "the mark should be at line 1");
			result.Column.Should().Be(1, "the mark should be at column 1");
		}
	}
}
