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
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace YamlDotNet.Analyzers.StaticGenerator
{
    static class SymbolExtensions
    {
        public static string GetFullName(this ITypeSymbol symbol, bool includeArrayDimensions = true)
        {
            if (symbol is IArrayTypeSymbol arrayTypeSymbol)
            {
                if (includeArrayDimensions)
                {
                    return $"{arrayTypeSymbol.ElementType.GetFullName()}[{(new string(',', arrayTypeSymbol.Rank - 1))}]";
                }
                return arrayTypeSymbol.ElementType.GetFullName();
            }

            if (symbol is INamedTypeSymbol namedTypeSymbol)
            {
                return GetFullName(namedTypeSymbol);
            }

            return symbol.Name;
        }

        public static string GetNamespace(this ISymbol symbol)
        {
            var parts = new Stack<string>();
            var currentNamespace = symbol.ContainingNamespace;

            while (currentNamespace != null)
            {
                if (!string.IsNullOrEmpty(currentNamespace.Name))
                {
                    parts.Push(currentNamespace.Name);
                    currentNamespace = currentNamespace.ContainingNamespace;
                }
                else
                {
                    break;
                }
            }

            return string.Join(".", parts);
        }

        static string GetNullable(this ITypeSymbol symbol)
        {
            if (symbol.IsValueType || symbol.NullableAnnotation != NullableAnnotation.Annotated)
            {
                return string.Empty;
            }

            return "?";
        }

        static string GetFullName(this INamedTypeSymbol symbol)
        {
            var space = GetNamespace(symbol);
            var suffix = GetGenericTypes(symbol.TypeArguments); ;

            var containingSymbol = symbol.ContainingType;
            while (containingSymbol != null)
            {
                if (string.IsNullOrWhiteSpace(space))
                {
                    space = containingSymbol.Name;
                }
                else
                {
                    space = $"{space}.{containingSymbol.Name}";
                }

                containingSymbol = containingSymbol.ContainingType;
            }

            if (!string.IsNullOrWhiteSpace(space))
            {
                return space + "." + symbol.Name + suffix + symbol.GetNullable();
            }
            else
            {
                return symbol.Name + suffix + symbol.GetNullable();
            }
        }

        static string GetGenericTypes(IReadOnlyList<ITypeSymbol> typeArguments)
        {
            var output = new List<string>();

            foreach (var argument in typeArguments)
            {
                switch (argument)
                {
                    case INamedTypeSymbol namedTypeSymbol:
                        output.Add(GetFullName(namedTypeSymbol));
                        break;
                    case ITypeParameterSymbol typeParameterSymbol:
                        output.Add(typeParameterSymbol.Name + typeParameterSymbol.GetNullable());
                        break;
                    case IArrayTypeSymbol arrayTypeSymbol:
                        output.Add(GetFullName(arrayTypeSymbol.ElementType) + "[]");
                        break;
                    default:
                        throw new NotSupportedException($"Cannot generate type name from type argument {argument.GetType().FullName}");
                }
            }

            if (output.Count > 0)
            {
                return $"<{string.Join(", ", typeArguments)}>";
            }

            return string.Empty;
        }
    }
}
