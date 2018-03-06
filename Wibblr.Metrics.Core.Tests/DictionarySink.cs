using System;
using System.Collections.Generic;

namespace Wibblr.Metrics.Core.Tests
{
    public class DictionarySink : IMetricsSink
    {
        public Dictionary<Metric, long> Counters { get; } = new Dictionary<Metric, long>();

        public void Flush(IEnumerable<WindowedCounter> counters)
        {
            foreach (var c in counters)
            {
                var key = new Metric(c.name, c.window);
                if (Counters.ContainsKey(key))
                    Counters[key] = Counters[key] + c.count;
                else
                    Counters[key] = c.count;
            }
        }

        public void Flush(IEnumerable<WindowedBucket> buckets)
        {
            throw new NotImplementedException();
        }

        public void Flush(IEnumerable<TimestampedEvent> events)
        {
            throw new NotImplementedException();
        }
    }
}
