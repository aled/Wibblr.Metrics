namespace Wibblr.Metrics.Plugins.CockroachDb
{
    public static class Extensions
    {
        public static string SqlQuote(this string s) =>
            "\"" + s.Replace("\"", "\\\"") + "\"";
    }
}
