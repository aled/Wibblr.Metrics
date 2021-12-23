using System;
using System.Collections.Generic;
using System.Linq;

namespace Wibblr.Utils
{
    public static class CollectionsExtensions
    {
        // returns number of items actually removed
        public static int DropLast<T>(this List<T> items, int count)
        {
            var numItemsToRemove = Math.Min(items.Count, count);
            var index = items.Count - numItemsToRemove;
            items.RemoveRange(index, numItemsToRemove);
            return numItemsToRemove;
        }

        public static IEnumerable<IEnumerable<T>> Batches<T>(this IEnumerable<T> items, int batchSize)
        {
            var batch = new List<T>();
            foreach (var item in items)
            {
                batch.Add(item);
                if (batch.Count == batchSize)
                    yield return batch;
            }
            if (batch.Count > 0)
                yield return batch;
        }

        public static IEnumerable<(T, int)> ZipWithIndex<T>(this IEnumerable<T> items)
        {
            var i = 0;
            foreach (var item in items)
                yield return (item, i++);
        }

        public static bool IsEmpty<T>(this IEnumerable<T> items) =>
            !items.Any();

        public static string Join(this IEnumerable<string> items, string separator = ", ") =>
            string.Join(separator, items);
    }
}
