using System;

namespace Wibblr.Metrics.Core
{
    public struct EventKey
    {
        public string name;
        public TimePeriod timePeriod;

        public EventKey(string name, DateTime start, DateTime end)
        {
            this.name = name;
            timePeriod = new TimePeriod(start, end);
        }
    }
}
