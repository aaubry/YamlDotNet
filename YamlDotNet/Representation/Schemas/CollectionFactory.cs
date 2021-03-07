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

using System;
using System.Linq;
using System.Linq.Expressions;

namespace YamlDotNet.Representation.Schemas
{
    public delegate TCollection CollectionFactory<TCollection>(int size);

    internal static class CollectionFactoryHelper
    {
        public static CollectionFactory<TCollection> CreateFactory<TCollection>()
        {
            CollectionFactory<TCollection> factory;

            var constructors = typeof(TCollection).GetConstructors();
            var intConstructor = constructors.FirstOrDefault(c =>
            {
                var parameters = c.GetParameters();
                return parameters.Length == 1 && parameters[0].ParameterType == typeof(int);
            });

            if (intConstructor != null)
            {
#if NET20
                factory = s => (TCollection)intConstructor.Invoke(new object[] { s });
#else
                var capacityParameter = Expression.Parameter(typeof(int), "s");
                var factoryExpression = Expression.Lambda<CollectionFactory<TCollection>>(
                    Expression.Convert(
                        Expression.New(intConstructor, capacityParameter),
                        typeof(TCollection)
                    ),
                    capacityParameter
                );
                factory = factoryExpression.Compile();
#endif
            }
            else
            {
                var defaultConstructor = constructors.FirstOrDefault(c => c.GetParameters().Length == 0);
                if (defaultConstructor == null)
                {
                    throw new ArgumentException($"The type '{typeof(TCollection).FullName}' must have a public constructor that takes an integer, or a public constructor without parameters.");
                }

#if NET20
                factory = _ => (TCollection)defaultConstructor.Invoke(null);
#else
                var capacityParameter = Expression.Parameter(typeof(int), "s");
                var factoryExpression = Expression.Lambda<CollectionFactory<TCollection>>(
                    Expression.Convert(
                        Expression.New(defaultConstructor),
                        typeof(TCollection)
                    ),
                    capacityParameter
                );
                factory = factoryExpression.Compile();
#endif
            }

            return factory;
        }
    }
}
