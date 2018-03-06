using System;

namespace Wibblr.Metrics.Core
{
    public class ProfilingEvent
    {
        public string sessionId; // e.g. hash of (hostname, request timestamp, request url)
        public string name;
        public DateTime timestamp;
        public int processId;
        public int threadId;
    }
}
