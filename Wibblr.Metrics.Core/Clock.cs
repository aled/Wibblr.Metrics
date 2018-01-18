using System;
namespace Wibblr.Metrics.Core
{
    public class Clock : IClock
    {
        public DateTime Current { get => DateTime.UtcNow; }
        public DateTime CurrentSeconds { get => Current.Truncate(TimeSpan.TicksPerSecond); }
    }
}
