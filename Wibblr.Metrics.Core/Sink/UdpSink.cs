using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Wibblr.Metrics.Plugins.Interfaces;

namespace Wibblr.Metrics.Core.Sink
{
    public class UdpSink : IMetricsSink
    {
        public UdpSink(IMetricsStreamSerializer serializer)
        {

        }

        public void Flush(IEnumerable<WindowedCounter> counters)
        {
            throw new NotImplementedException();
        }

        public void Flush(IEnumerable<WindowedBucket> buckets)
        {
            throw new NotImplementedException();
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
