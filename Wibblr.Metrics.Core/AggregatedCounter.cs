using System;

namespace Wibblr.Metrics.Core
{
    public struct AggregatedCounter
    {
        public string name;
        public Window window;
        public long count;
    }
}
