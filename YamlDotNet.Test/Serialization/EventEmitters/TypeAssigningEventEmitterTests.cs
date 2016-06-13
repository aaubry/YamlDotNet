using FakeItEasy;
using Xunit;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.EventEmitters;

namespace YamlDotNet.Test.Serialization.EventEmitters
{
    public class TypeAssigningEventEmitterTests
    {
        private readonly TypeAssigningEventEmitter _emitter;

        public TypeAssigningEventEmitterTests()
        {
            var nextEmitterMock = A.Fake<IEventEmitter>();
            _emitter = new TypeAssigningEventEmitter(nextEmitterMock, false);
        }

        [Fact]
        void Emit_StringValueSingleQuote_SetsStyleToDoubleQuoted()
        {
            string value = "'";
            var info = new ScalarEventInfo(CreateObjectDescriptor(value));
            
            _emitter.Emit(info);
            
            Assert.Equal(ScalarStyle.DoubleQuoted, info.Style);
        }

        [Fact]
        void Emit_StringValueTextWithSingleQuotes_SetsStyleToDoubleQuoted()
        {
            string value = "'asdf'";
            var info = new ScalarEventInfo(CreateObjectDescriptor(value));

            _emitter.Emit(info);

            Assert.Equal(ScalarStyle.DoubleQuoted, info.Style);
        }

        [Fact]
        void Emit_CharValueSingleQuote_SetsStyleToDoubleQuoted()
        {
            char value = '\'';
            var info = new ScalarEventInfo(CreateObjectDescriptor(value));

            _emitter.Emit(info);

            Assert.Equal(ScalarStyle.DoubleQuoted, info.Style);
        }
        
        [Fact]
        void Emit_StringValueWithNoSingleQuotes_SetsStyleToAny()
        {
            string value = "test";
            var info = new ScalarEventInfo(CreateObjectDescriptor(value));

            _emitter.Emit(info);

            Assert.Equal(ScalarStyle.Any, info.Style);
        }

        [Fact]
        void Emit_CharValueWithNoSingleQuotes_SetsStyleToAny()
        {
            char value = 'a';
            var info = new ScalarEventInfo(CreateObjectDescriptor(value));

            _emitter.Emit(info);

            Assert.Equal(ScalarStyle.Any, info.Style);
        }


        private IObjectDescriptor CreateObjectDescriptor<T>(T value)
        {
            var objectDescriptor = A.Fake<IObjectDescriptor>();
            A.CallTo(() => objectDescriptor.Type).Returns(typeof(T));
            A.CallTo(() => objectDescriptor.Value).Returns(value);
            A.CallTo(() => objectDescriptor.ScalarStyle).Returns(ScalarStyle.Any);
            return objectDescriptor;
        }

        private string EscapedValue(string value)
        {
            return string.Format("\"{0}\"", value);
        }
    }
}
