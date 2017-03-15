using System;
using System.Collections.Generic;
using System.Linq;

namespace AzureBlobExport
{
    internal static class IEnumerableOfTExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
        {
            if(items == null)
            {
                return;
            }

            foreach (var obj in items)
            {
                action(obj);
            }
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> items)
        {
            return items == null || !items.Any();
        }

        public static IEnumerable<Tuple<int, IEnumerable<T>>> Split<T>(this IEnumerable<T> source, int count)
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / count)
                .Select(x => new Tuple<int, IEnumerable<T>>(x.Key, x.Select(v => v.Value).ToList()));
        }
    }
}
