using System;
using System.Collections.Generic;
using System.Linq;

using Wibblr.Metrics.Plugins.Interfaces;

namespace Wibblr.Metrics.Core.Tests
{
    public class DictionarySink : IMetricsSink
    {
        public Dictionary<Metric, long> Counters { get; } = 
            new Dictionary<Metric, long>();
        
        public Dictionary<Metric, Dictionary<(int, int), long>> Buckets { get; } = 
            new Dictionary<Metric, Dictionary<(int, int), long>>();

        public void Flush(IEnumerable<WindowedCounter> counters)
        {
            foreach (var c in counters)
            {
                var key = new Metric(c.name, c.from, c.to);
                if (Counters.ContainsKey(key))
                    Counters[key] = Counters[key] + c.count;
                else
                    Counters[key] = c.count;
            }
        }

        public void Flush(IEnumerable<WindowedBucket> windowedBuckets)
        {
            foreach (var wb in windowedBuckets)
            {
                var metric = new Metric(wb.name, wb.timeFrom, wb.timeTo);
                var bucketKey = (wb.valueFrom ?? int.MinValue, wb.valueTo ?? int.MaxValue);

                if (Buckets.TryGetValue(metric, out Dictionary<(int, int), long> histogram))
                {
                    if (histogram.TryGetValue(bucketKey, out long count))
                        histogram[bucketKey] += wb.count;
                    else
                        histogram[bucketKey] = wb.count;
                }
                else
                {
                    Buckets[metric] = new Dictionary<(int, int), long> { { bucketKey, wb.count } };  
                }
            }
        }

        public void Flush(IEnumerable<TimestampedEvent> events)
        {
            throw new NotImplementedException();
        }

        public void Flush(IEnumerable<Profile> profiles)
        {
            throw new NotImplementedException();
        }

        public void FlushComplete()
        {
            throw new NotImplementedException();
        }
    }
}
