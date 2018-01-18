using System;
namespace Wibblr.Metrics.Core
{
    public static class Extensions
    {
        public static DateTime Truncate(this DateTime dateTime, long unitTicks) =>
            new DateTime(dateTime.Ticks - (dateTime.Ticks % unitTicks), DateTimeKind.Utc);

        public static string ToIsoString(this DateTime dateTime) =>
            dateTime.ToString("yyyy-MM-dd HH:mm:ss");
    }
}
