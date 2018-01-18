using System;
using System.Collections.Generic;
using System.Text;

namespace Wibblr.Metrics.Core
{
    public struct AggregatedEvent
    {
        public string name;
        public DateTime startTime;
        public DateTime endTime;
        public long count;
    }
}
