using System;
using System.Collections.Generic;

namespace Wibblr.Collections
{
    public static class Extensions
    {
        public static List<T> DropLast<T>(this List<T> items, int count)
        {
            var numItemsToRemove = Math.Min(items.Count, count);
            var index = items.Count - numItemsToRemove;
            items.RemoveRange(index, numItemsToRemove);
            return items;
        }
    }
}
