using System;
using System.Collections.Generic;

namespace YamlDotNet.Test
{
	public static class EnumerableExtensions
	{
		public static IEnumerable<T> Do<T>(this IEnumerable<T> source, Action<T> action)
		{
			foreach (var item in source)
			{
				action(item);
				yield return item;
			}
		}

		public static void Run<T>(this IEnumerable<T> source, Action<T> action)
		{
			foreach (var element in source)
			{
				action(element);
			}
		}
	}
}