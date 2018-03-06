using System;
using System.Diagnostics;
using System.Threading;

namespace Wibblr.Metrics.Core
{
    /// <summary>
    /// Profiling interval
    /// </summary>
    public class ProfilingInterval 
    {
        public MetricsCollector metrics;
        public string sessionId; // e.g. hash of (hostname, request timestamp, request url)
        public string name;
        public DateTime timestamp;
        public int processId;
        public int threadId;
        public DateTime endTimestamp;
    }
}
