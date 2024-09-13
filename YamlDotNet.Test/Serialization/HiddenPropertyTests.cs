// This file is part of YamlDotNet - A .NET library for YAML.
// Copyright (c) Antoine Aubry and contributors
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
// of the Software, and to permit persons to whom the Software is furnished to do
// so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using FluentAssertions;
using Xunit;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace YamlDotNet.Test.Serialization
{
    public class HiddenPropertyTests
    {
        public class HiddenPropertyBase
        {
            protected object value;

            public object Value => this.value;

            [YamlIgnore]
            public object SetOnly { set => this.value = value; }

            [YamlIgnore]
            public object GetAndSet { get; set; }
        }

        public class HiddenPropertyDerived<T> : HiddenPropertyBase
        {
            public new T Value { get => (T)this.value; }
            public new T SetOnly { set => this.value = value; }
            public new T GetAndSet { get; set; }
        }

        public class DuplicatePropertyBase
        {
            protected object value;

            public object Value => this.value;
            public object SetOnly { set => this.value = value; }
            public object GetAndSet { get; set; }
        }

        public class DuplicatePropertyDerived<T> : DuplicatePropertyBase
        {
            public new T Value { get => (T)this.value; }
            public new T SetOnly { set => this.value = value; }
            public new T GetAndSet { get; set; }
        }

        [Fact]
        public void TestHidden()
        {
            var yaml = @"
setOnly: set
getAndSet: getAndSet
";
            var d = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var o = d.Deserialize<HiddenPropertyDerived<string>>(yaml);
            Assert.Equal("set", o.Value);
            Assert.Equal("getAndSet", o.GetAndSet);
        }

        [Fact]
        public void TestDuplicate()
        {
            var yaml = @"
setOnly: set
getAndSet: getAndSet
";
            var d = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            Action action = () => { d.Deserialize<DuplicatePropertyDerived<string>>(yaml); };
            action.ShouldThrow<YamlException>();
        }
    }
}
