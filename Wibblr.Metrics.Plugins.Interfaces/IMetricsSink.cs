using System.Collections.Generic;

namespace Wibblr.Metrics.Plugins.Interfaces
{
    public interface IMetricsSink
    {
        void Flush(IEnumerable<WindowedCounter> counters);
        void Flush(IEnumerable<WindowedBucket> buckets);
        void Flush(IEnumerable<TimestampedEvent> events);
        void Flush(IEnumerable<Profile> profiles);
        void FlushComplete();
    }
}
