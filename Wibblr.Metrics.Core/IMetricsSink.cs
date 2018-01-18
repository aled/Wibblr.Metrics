using System;
using System.Collections.Generic;

namespace Wibblr.Metrics.Core
{
    public interface IMetricsSink
    {
        void RecordEvents(IEnumerable<AggregatedEvent> events);
    }
}
