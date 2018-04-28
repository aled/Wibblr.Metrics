using System;

namespace Wibblr.Metrics.Core
{
    public struct Window
    {
        public DateTime start;
        public TimeSpan size;

        public Window(DateTime start, TimeSpan size)
        {
            this.start = start.RoundDown(size);
            this.size = size;
        }

        public DateTime end { get => start.Add(size); }
    }
}
