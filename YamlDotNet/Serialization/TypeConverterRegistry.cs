using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace YamlDotNet.Serialization
{
    public sealed class TypeConverterRegistry
    {
        private readonly Dictionary<(Type from, Type to), Func<object, object>> converters = new Dictionary<(Type from, Type to), Func<object, object>>();

        public void Add(Type from, Type to, Func<object, object> converter)
        {
            converters.Add((from, to), converter);
        }

        public void Add<TFrom, TTo>(Func<TFrom, TTo> converter)
        {
            Add(typeof(TFrom), typeof(TTo), f => converter((TFrom)f)!);
        }

        public bool TryGetConverter(Type from, Type to, [NotNullWhen(true)] out Func<object, object>? converter)
        {
            return converters.TryGetValue((from, to), out converter);
        }

        //public bool TryGetConverter(Type from, Type to, out Func<object, object>? converter)
        //{
        //}
    }
}
