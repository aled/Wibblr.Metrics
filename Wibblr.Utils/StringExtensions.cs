using System;
using System.Linq;

namespace Wibblr.Utils
{
    public static class StringExtensions
    {
        public static bool IsAlphanumeric(this string s) =>
              s.All(c => char.IsLetterOrDigit(c));
    }
}
