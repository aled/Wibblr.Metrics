using System;
using System.Collections.Generic;

namespace Wibblr.Metrics.Core
{
    public class DictionarySink : IMetricsSink
    {
        public Dictionary<Metric, long> Counters { get; } = new Dictionary<Metric, long>();

        public void Flush(IEnumerable<AggregatedCounter> counters)
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
    }
}
