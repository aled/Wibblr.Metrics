using System;
using System.Collections.Generic;

namespace Wibblr.Metrics.Core
{
    public class DictionarySink : IMetricsSink
    {
        public Dictionary<EventKey, long> Events { get; } = new Dictionary<EventKey, long>();

        public void RecordEvents(IEnumerable<AggregatedEvent> events)
        {
            foreach (var e in events)
            {
                var key = new EventKey(e.name, e.startTime, e.endTime);
                if (Events.ContainsKey(key))
                    Events[key] = Events[key] + e.count;
                else
                    Events[key] = e.count;
            }
        }
    }
}
