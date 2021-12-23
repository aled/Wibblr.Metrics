using System;

namespace Wibblr.Metrics.Plugins.SqlServer
{
    public static class Extensions
    {
        public static string SqlQuote(this string s) => 
            "[" + s.Replace("]", "]]") + "]";
    }
}
