using FakeItEasy;
using Xunit;
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
        void Emit_StringValueWithSingleQuote_EscapedByDoubleQuotes()
        {
            string value = "'test";
            var info = new ScalarEventInfo(CreateObjectDescriptor(value));
            
            _emitter.Emit(info);
            
            Assert.Equal(EscapedValue(value), info.RenderedValue);
        }

        [Fact]
        void Emit_CharValueWithSingleQuote_EscapedByDoubleQuotes()
        {
            char value = '\'';
            var info = new ScalarEventInfo(CreateObjectDescriptor(value));

            _emitter.Emit(info);

            Assert.Equal(EscapedValue(value.ToString()), info.RenderedValue);
        }

        [Fact]
        void Emit_StringTypeWithNoSingleQuotes_RemainsUnchanged()
        {
            string value = "test";
            var info = new ScalarEventInfo(CreateObjectDescriptor(value));

            _emitter.Emit(info);

            Assert.Equal(value, info.RenderedValue);
        }

        [Fact]
        void Emit_CharTypeWithNoSingleQuotes_RemainsUnchanged()
        {
            char value = 'a';
            var info = new ScalarEventInfo(CreateObjectDescriptor(value));

            _emitter.Emit(info);

            Assert.Equal(value.ToString(), info.RenderedValue);
        }


        private IObjectDescriptor CreateObjectDescriptor<T>(T value)
        {
            var objectDescriptor = A.Fake<IObjectDescriptor>();
            A.CallTo(() => objectDescriptor.Type).Returns(typeof(T));
            A.CallTo(() => objectDescriptor.Value).Returns(value);
            return objectDescriptor;
        }

        private string EscapedValue(string value)
        {
            return string.Format("\"{0}\"", value);
        }
    }
}
