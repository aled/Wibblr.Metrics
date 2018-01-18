using System;
namespace Wibblr.Metrics.Core
{
    internal static class Extensions
    {
        public static DateTime Truncate(this DateTime dateTime, long unitTicks) =>
            new DateTime(dateTime.Ticks - (dateTime.Ticks % unitTicks), DateTimeKind.Utc);

        public static DateTime Seconds(this DateTime dateTime) =>
            dateTime.Truncate(TimeSpan.TicksPerSecond);
    }
}
