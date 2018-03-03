using System;
using System.Collections.Generic;

namespace Wibblr.Metrics.Core
{
    public interface IMetricsSink
    {
        void Flush(IEnumerable<AggregatedCounter> counters);
        void Flush(IEnumerable<WindowedBucket> buckets);
    }
}
