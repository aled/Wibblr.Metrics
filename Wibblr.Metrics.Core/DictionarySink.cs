using System;
using System.Collections.Generic;

namespace Wibblr.Metrics.Core
{
    public class DictionarySink : IMetricsSink
    {
        public Dictionary<EventKey, long> Events { get; } = new Dictionary<EventKey, long>();

        public void RecordEvents(IDictionary<EventKey, long> events)
        {
            foreach (var kv in events)
            {
                var key = kv.Key;
                var count = kv.Value;

                if (Events.ContainsKey(key))
                    Events[key] = Events[key] + count;
                else
                    Events[key] = count;
            }
        }
    }
}
