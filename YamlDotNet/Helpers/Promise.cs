using System;
using System.Diagnostics.CodeAnalysis;

namespace YamlDotNet.Helpers
{
    internal sealed class Promise<T> where T : class
    {
        private T? value;

        public Promise(T value)
        {
            if (value is null)
            {
                throw new ArgumentNullException();
            }

            this.value = value;
        }

        public Promise() { }

        bool TryGetValue([NotNullWhen(true)] out T? value)
        {
            value = this.value;
            return !(value is null);
        }

        public void SetValue(T value)
        {
            if (value is null)
            {
                throw new ArgumentNullException();
            }

            if (!(this.value is null))
            {
                throw new InvalidOperationException("Attempted to the the value of a promise that already has a value.");
            }

            this.value = value;
            ValueAvailable?.Invoke(value);
        }

        public event Action<T>? ValueAvailable;

        public static implicit operator Promise<T>(T value) => new Promise<T>(value);
    }
}
