//  This file is part of YamlDotNet - A .NET library for YAML.
//  Copyright (c) Antoine Aubry and contributors

//  Permission is hereby granted, free of charge, to any person obtaining a copy of
//  this software and associated documentation files (the "Software"), to deal in
//  the Software without restriction, including without limitation the rights to
//  use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
//  of the Software, and to permit persons to whom the Software is furnished to do
//  so, subject to the following conditions:

//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.

//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.

namespace System
{
    internal sealed class Lazy<T>
    {
        private enum ValueState
        {
            NotCreated,
            Creating,
            Created,
        }

        private T value;
        private Func<T>? valueFactory;
        private ValueState valueState;
        private readonly bool isThreadSafe;

        public Lazy(T value)
        {
            this.value = value;
            valueState = ValueState.Created;
        }

        public Lazy(Func<T> valueFactory, bool isThreadSafe)
        {
            this.valueFactory = valueFactory;
            this.isThreadSafe = isThreadSafe;
            valueState = ValueState.NotCreated;
            value = default!;
        }

        public T Value
        {
            get
            {
                if (isThreadSafe)
                {
                    lock (this)
                    {
                        return ComputeValue();
                    }
                }
                else
                {
                    return ComputeValue();
                }
            }
        }

        private T ComputeValue()
        {
            switch (valueState)
            {
                case Lazy<T>.ValueState.NotCreated:
                    valueState = ValueState.Creating;
                    value = valueFactory();
                    valueState = ValueState.Created;
                    valueFactory = null;
                    break;

                case Lazy<T>.ValueState.Creating:
                    throw new InvalidOperationException("ValueFactory attempted to access the Value property of this instance.");
            }
            return value;
        }
    }
}
