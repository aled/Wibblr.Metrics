using System;

namespace Wibblr.Utils
{
    public static class DateTimeExtensions
    {
        public static DateTime RoundDown(this DateTime dateTime, TimeSpan timeSpan) =>
               new DateTime(dateTime.Ticks - (dateTime.Ticks % timeSpan.Ticks), DateTimeKind.Utc);

        public static DateTime RoundUp(this DateTime dateTime, TimeSpan timeSpan) =>
            new DateTime(dateTime.Ticks - (dateTime.Ticks % timeSpan.Ticks) + timeSpan.Ticks, DateTimeKind.Utc);

        public static DateTime Seconds(this DateTime dateTime) =>
            dateTime.RoundDown(TimeSpan.FromSeconds(1));

        public static bool IsDivisorOf(this TimeSpan part, TimeSpan whole) =>
            whole.Ticks % part.Ticks == 0;
    }
}
