using System;
using System.Collections.Generic;
using System.Linq;

namespace Extensions
{
    public static class EnumerableExtensions
    {
        public static T ZeroOrSingle<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate)
        {
            var elements = enumerable.Where(predicate).ToList();

            if (elements.Count > 1)
                throw new InvalidOperationException();

            return elements.SingleOrDefault();
        }

        public static T SelectSingle<T, U>(this IEnumerable<U> enumerable, Func<U, T> selector)
        {
            return enumerable.Select(selector).Distinct().Single();
        }

        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var element in enumerable)
                action(element);
        }
    }
}
