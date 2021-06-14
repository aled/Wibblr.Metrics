using System;

using Wibblr.Metrics.Plugins.Interfaces;

namespace Wibblr.Metrics.Core
{
    /// <summary>
    /// Name files according to the date/time of the metric. Actual name is
    /// configurable using a formatting string, so can store by minute, hour or whatever.
    /// </summary>
    public class DateTimeFileNamingStrategy : IFileNamingStrategy
    {
        private string formatString;

        public DateTimeFileNamingStrategy(string formatString = null)
        {
            if (string.IsNullOrWhiteSpace(formatString))
                formatString = "yyyyMMdd-HHmm";

            this.formatString = formatString;
        }

        private string Format(DateTime timestamp) => timestamp.ToString(formatString);

        private string Format(string metricName, DateTime timestamp) => $"{metricName}-{Format(timestamp)}";

        private bool EqualNames(DateTime a, DateTime b) => a == b || Format(a) == Format(b);

        public string Basename(WindowedCounter counter) => Format("counter", counter.from);

        public bool EqualNames(WindowedCounter a, WindowedCounter b) => EqualNames(a.from, b.from);

        public string Basename(TimestampedEvent timestampedEvent) => Format("event", timestampedEvent.timestamp);

        public bool EqualNames(TimestampedEvent a, TimestampedEvent b) => EqualNames(a.timestamp, b.timestamp);

        public string Basename(WindowedBucket bucket) => Format("histogram", bucket.timeFrom);

        public bool EqualNames(WindowedBucket a, WindowedBucket b) => EqualNames(a.timeFrom, b.timeFrom);

        public string Basename(Profile profile) => Format("profile", profile.timestamp);

        public bool EqualNames(Profile a, Profile b) => EqualNames(a.timestamp, b.timestamp);
    }
}
