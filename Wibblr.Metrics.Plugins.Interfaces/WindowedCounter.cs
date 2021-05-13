using System;

namespace Wibblr.Metrics.Plugins.Interfaces
{
    public struct WindowedCounter
    {
        public string name;
        public DateTime from;
        public DateTime to;
        public long count;
    }
}
