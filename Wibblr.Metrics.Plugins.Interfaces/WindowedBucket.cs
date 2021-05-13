using System;

namespace Wibblr.Metrics.Plugins.Interfaces
{
    public struct WindowedBucket
    {
        public string name;
        public DateTime timeFrom;
        public DateTime timeTo;
        public int? valueFrom;
        public int? valueTo;
        public long count;
    }
}
