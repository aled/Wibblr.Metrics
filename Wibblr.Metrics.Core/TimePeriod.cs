using System;

namespace Wibblr.Metrics.Core
{
    public struct TimePeriod
    {
        public DateTime start;
        public DateTime end;

        public TimePeriod(DateTime start, DateTime end)
        {
            this.start = start;
            this.end = end;
        }
    }
}
