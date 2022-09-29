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
using System.Collections;
using System.ComponentModel;
using YamlDotNet.Core;

namespace YamlDotNet.Serialization.ObjectGraphVisitors
{
    public sealed class DefaultValuesObjectGraphVisitor : ChainedObjectGraphVisitor
    {
        private readonly DefaultValuesHandling handling;

        public DefaultValuesObjectGraphVisitor(DefaultValuesHandling handling, IObjectGraphVisitor<IEmitter> nextVisitor)
            : base(nextVisitor)
        {
            this.handling = handling;
        }

        private static object? GetDefault(Type type)
        {
            return type.IsValueType() ? Activator.CreateInstance(type) : null;
        }

        public override bool EnterMapping(IPropertyDescriptor key, IObjectDescriptor value, IEmitter context)
        {
            var configuration = handling;
            var yamlMember = key.GetCustomAttribute<YamlMemberAttribute>();
            if (yamlMember != null && yamlMember.IsDefaultValuesHandlingSpecified)
            {
                configuration = yamlMember.DefaultValuesHandling;
            }

            if ((configuration & DefaultValuesHandling.OmitNull) != 0)
            {
                if (value.Value is null)
                {
                    return false;
                }
            }

            if ((configuration & DefaultValuesHandling.OmitEmptyCollections) != 0)
            {
                if (value.Value is IEnumerable enumerable)
                {
                    var enumerator = enumerable.GetEnumerator();
                    var canMoveNext = enumerator.MoveNext();
                    if (enumerator is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }

                    if (!canMoveNext)
                    {
                        return false;
                    }
                }
            }

            if ((configuration & DefaultValuesHandling.OmitDefaults) != 0)
            {
                var defaultValue = key.GetCustomAttribute<DefaultValueAttribute>()?.Value ?? GetDefault(key.Type);
                if (Equals(value.Value, defaultValue))
                {
                    return false;
                }
            }

            return base.EnterMapping(key, value, context);
        }
    }
}
