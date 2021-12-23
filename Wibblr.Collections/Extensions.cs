using System;
using System.Collections.Generic;
using System.Linq;

namespace Wibblr.Collections
{
    public static class Extensions
    {
        public static LookAheadEnumerator<T> GetLookAheadEnumerator<T>(this IEnumerable<T> items)
        {
            return new LookAheadEnumerator<T>(items);
        }

        // returns number of items actually removed
        public static int DropLast<T>(this List<T> items, int count)
        {
            var numItemsToRemove = Math.Min(items.Count, count);
            var index = items.Count - numItemsToRemove;
            items.RemoveRange(index, numItemsToRemove);
            return numItemsToRemove;
        }

        private static IEnumerable<T> TakeUntil<T>(this LookAheadEnumerator<T> e, Func<T, T, bool> predicate)
        {
            while (e.HasCurrent)
            {
                yield return e.Current;

                if (e.HasNext && predicate(e.Current, e.Next))
                    break;

                e.MoveNext();
            }
        }

        /// <summary>
        /// Split an IEnumerable into batches based on a predicate
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="partitionPredicate"></param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<T>> SplitAt<T>(this IEnumerable<T> items, Func<T, T, bool> partitionPredicate)
        {
            var enumerator = items.GetLookAheadEnumerator();

            while (enumerator.MoveNext())
                yield return enumerator.TakeUntil(partitionPredicate);
        }
    }
}
