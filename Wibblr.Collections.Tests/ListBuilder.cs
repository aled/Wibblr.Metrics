using System.Collections.Generic;
using System.Linq;

namespace Wibblr.Collections.Tests
{
    public static class ListBuilder<T>
    {
        public static List<T> L(params T[] values) => values.ToList();
    }
}
