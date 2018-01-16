using System;
using System.Collections.Generic;

namespace Wibblr.Metrics.Core
{
    public interface IMetricsSink
    {
        void RecordEvents(IDictionary<EventKey, int> events);
    }
}
    