namespace Xunit.Abstractions
{
    public interface ITestOutputHelper {
        void WriteLine();
        void WriteLine(string value);
        void WriteLine(string format, params object[] args);
    }
}
