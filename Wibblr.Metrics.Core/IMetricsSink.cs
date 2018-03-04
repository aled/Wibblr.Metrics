using System;
using System.Collections.Generic;

namespace Wibblr.Metrics.Core
{
    public interface IMetricsSink
    {
        void Flush(IEnumerable<WindowedCounter> counters);
        void Flush(IEnumerable<WindowedBucket> buckets);
    }
}
