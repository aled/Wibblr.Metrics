using System;

namespace Wibblr.Metrics.Core
{
    public struct EventKey
    {
        public string name;
        public TimePeriod timePeriod;

        public EventKey(string name, DateTime startTime, DateTime endTime)
        {
            this.name = name;
            timePeriod = new TimePeriod(startTime, endTime);
        }
    }
}
